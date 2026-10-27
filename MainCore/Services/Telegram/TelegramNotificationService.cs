using System.Collections.Concurrent;
using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Serilog.Events;

namespace MainCore.Services.Telegram;

[RegisterSingleton<TelegramNotificationService>]
public sealed class TelegramNotificationService(ITelegramSettingsStore settings, ITelegramTransport transport, RxQueue notifications) : BackgroundService
{
    private sealed record Pending(TelegramAlert Alert, string Revision, string Text, DateTimeOffset Created);
    private readonly Channel<Pending> _pending = Channel.CreateBounded<Pending>(new BoundedChannelOptions(256) { SingleReader = true, FullMode = BoundedChannelFullMode.Wait });
    private readonly ConcurrentDictionary<int, string> _delivery = new();
    private readonly Dictionary<string, DateTimeOffset> _recent = [];
    private readonly TelegramDetector _detector = new();
    private readonly object _gate = new();
    private readonly SemaphoreSlim _sendGate = new(1, 1);
    private readonly List<IDisposable> _subscriptions = [];
    private DateTimeOffset _nextSend;
    private bool _accepting;
    private sealed class Observer<T>(Action<T> action) : IObserver<T>
    {
        public void OnNext(T value) => action(value);
        public void OnCompleted() { }
        public void OnError(Exception error) { }
    }
    public ITelegramSettingsStore Settings => settings;
    public TelegramOptions SaveSettings(AccountId accountId, TelegramOptions options, string token)
    {
        lock (_gate)
        {
            var saved = settings.Save(accountId, options, token);
            _detector.Reset(accountId);
            return saved;
        }
    }
    public string DeliveryStatus(AccountId accountId) => _delivery.GetValueOrDefault(accountId.Value, "No delivery attempted during this session.");

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _accepting = true;
        _subscriptions.Add(notifications.GetObservable<TelegramAlert>().Subscribe(new Observer<TelegramAlert>(Publish)));
        _subscriptions.Add(notifications.GetObservable<BuildPlanApplied>().Subscribe(new Observer<BuildPlanApplied>(plan =>
            Publish(new(plan.AccountId, plan.VillageId, TelegramEvents.AiPlanApplied,
                $"AI building plan applied: {plan.UpgradeCount} upgrades. Goal: {plan.Goal}. Queue mode: {plan.QueueMode}.", plan.PlanId)))));
        _subscriptions.Add(notifications.GetObservable<TelegramVillageScan>().Subscribe(new Observer<TelegramVillageScan>(scan =>
        {
            lock (_gate) foreach (var alert in _detector.Observe(scan)) Publish(alert);
        })));
        _subscriptions.Add(notifications.GetObservable<StatusModified>().Subscribe(new Observer<StatusModified>(status =>
        {
            lock (_gate) { var alert = _detector.Status(status); if (alert is not null) Publish(alert); }
        })));
        _subscriptions.Add(notifications.GetObservable<LogEmitted>().Subscribe(new Observer<LogEmitted>(log =>
        {
            if (log.LogEvent.Level >= LogEventLevel.Error)
                Publish(new(log.AccountId, null, TelegramEvents.Errors, "An error was logged. Open the account's bot log for details. Raw logs are not sent to Telegram.", "error"));
        })));
        return base.StartAsync(cancellationToken);
    }

    public void Publish(TelegramAlert alert)
    {
        if (!_accepting) return;
        try
        {
            var options = settings.Load(alert.AccountId);
            if (!options.Enabled || (options.Events & alert.Kind) == 0) return;
            var now = DateTimeOffset.UtcNow;
            var key = $"{alert.AccountId.Value}:{alert.VillageId?.Value}:{alert.Kind}:{alert.Key}:{options.Revision}";
            lock (_gate)
            {
                foreach (var expired in _recent.Where(x => now - x.Value > TimeSpan.FromMinutes(10)).Select(x => x.Key).ToArray()) _recent.Remove(expired);
                if (_recent.ContainsKey(key)) return;
                var text = $"Travian Bot · {alert.Kind}\n{settings.Describe(alert.AccountId, alert.VillageId)}\n\n{alert.Text}\n\nObserved {now:yyyy-MM-dd HH:mm:ss} UTC";
                if (!_pending.Writer.TryWrite(new(alert, options.Revision, text, now)))
                {
                    _delivery[alert.AccountId.Value] = "Notification queue is full; an alert was skipped.";
                    return;
                }
                _recent[key] = now;
            }
        }
        catch { _delivery[alert.AccountId.Value] = "Could not queue this alert. Check Telegram settings and the account."; }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var pending in _pending.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    await SendPacedAsync(async () =>
                    {
                        var options = settings.Load(pending.Alert.AccountId);
                        if (!options.Enabled || options.Revision != pending.Revision || (options.Events & pending.Alert.Kind) == 0) return;
                        if (DateTimeOffset.UtcNow - pending.Created > TimeSpan.FromMinutes(10))
                        {
                            _delivery[pending.Alert.AccountId.Value] = "An alert expired in the delivery queue and was skipped.";
                            return;
                        }
                        // Check ownership and load the current credential immediately before sending.
                        settings.Describe(pending.Alert.AccountId, pending.Alert.VillageId);
                        await transport.SendAsync(settings.ReadToken(pending.Alert.AccountId, options), options.ChatId, pending.Text, options.Silent, stoppingToken);
                        _delivery[pending.Alert.AccountId.Value] = $"Last alert delivered at {DateTime.Now:HH:mm:ss}.";
                    }, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
                catch (TelegramDeliveryException ex) { _delivery[pending.Alert.AccountId.Value] = ex.Message; }
                catch { _delivery[pending.Alert.AccountId.Value] = "Delivery failed. Check the bot token, chat ID and account settings."; }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
    }

    private async Task SendPacedAsync(Func<Task> send, CancellationToken token)
    {
        await _sendGate.WaitAsync(token);
        try
        {
            for (var attempt = 0; ; attempt++)
            {
                while (_nextSend > DateTimeOffset.UtcNow)
                {
                    var wait = _nextSend - DateTimeOffset.UtcNow;
                    if (wait > TimeSpan.Zero) await Task.Delay(wait < TimeSpan.FromMinutes(1) ? wait : TimeSpan.FromMinutes(1), token);
                }
                _nextSend = DateTimeOffset.UtcNow.AddSeconds(3.1); // Also respects Telegram's group limit of 20 messages/minute.
                try { await send(); return; }
                catch (TelegramDeliveryException ex) when (ex.RetryAfter > 0)
                {
                    _nextSend = DateTimeOffset.UtcNow.AddSeconds(Math.Max(3.1, ex.RetryAfter));
                    if (attempt > 0 || ex.RetryAfter > 300) throw;
                }
            }
        }
        finally { _sendGate.Release(); }
    }

    public Task<TelegramChat[]> FindChatsAsync(string token, CancellationToken cancellationToken) => transport.FindChatsAsync(token, cancellationToken);
    public async Task TestAsync(AccountId accountId, string token, string chatId, bool silent, CancellationToken cancellationToken)
    {
        var text = $"Travian Bot test notification\n{settings.Describe(accountId, null)}\nTelegram delivery is working. Choose your notification types and Save to enable automatic alerts.";
        await SendPacedAsync(() => transport.SendAsync(token, chatId, text, silent, cancellationToken), cancellationToken);
        _delivery[accountId.Value] = $"Test delivered at {DateTime.Now:HH:mm:ss}.";
    }
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _accepting = false;
        foreach (var subscription in _subscriptions) subscription.Dispose();
        _subscriptions.Clear();
        _pending.Writer.TryComplete();
        await base.StopAsync(cancellationToken);
    }
}
