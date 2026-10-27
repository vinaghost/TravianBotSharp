using System.Collections.Concurrent;
using System.Threading.Channels;
using MainCore.Entities;
using MainCore.Infrasturecture.Persistence;
using MainCore.Services;
using MainCore.Services.Telegram;
using Microsoft.EntityFrameworkCore;

namespace MainCore.Test.Telegram;

public class TelegramDeliveryTests
{
    private sealed class Store : ITelegramSettingsStore
    {
        public ConcurrentDictionary<int, TelegramOptions> Options { get; } = new();
        public TelegramOptions Load(AccountId id) => Options.GetValueOrDefault(id.Value, new());
        public string ReadToken(AccountId id, TelegramOptions options) => "fake-token";
        public TelegramOptions Save(AccountId id, TelegramOptions options, string token) => Options[id.Value] = options with { Revision = Guid.NewGuid().ToString("N") };
        public string Describe(AccountId id, VillageId? village) => $"Account {id.Value}, village {village?.Value}";
    }
    private sealed class Transport : ITelegramTransport
    {
        public Channel<(string Chat, string Text)> Sent { get; } = Channel.CreateUnbounded<(string, string)>();
        public Func<Task>? OnSend { get; set; }
        public int Calls;
        public async Task SendAsync(string token, string chatId, string text, bool silent, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref Calls);
            await Sent.Writer.WriteAsync((chatId, text), cancellationToken);
            if (OnSend is not null) await OnSend();
        }
        public Task<TelegramChat[]> FindChatsAsync(string token, CancellationToken cancellationToken) => Task.FromResult(Array.Empty<TelegramChat>());
    }
    private static TelegramOptions Enabled(string chat = "42") => new() { Enabled = true, ChatId = chat, Events = TelegramEvents.IncomingAttack, Revision = Guid.NewGuid().ToString("N") };
    private static TelegramAlert Alert(int account = 1, string key = "attack") => new(new(account), new(1), TelegramEvents.IncomingAttack, "Incoming attack", key);
    private static RxQueue Bus() => new(Substitute.For<IDbContextFactory<AppDbContext>>());
    private static async Task Until(Func<bool> condition)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while (!condition()) await Task.Delay(20, timeout.Token);
    }

    [Fact]
    public async Task BuildPlanNotificationsUseTheSharedEventWithoutAnAiServiceDependency()
    {
        var store = new Store();
        store.Options[1] = Enabled() with { Events = TelegramEvents.AiPlanApplied };
        var transport = new Transport(); var bus = Bus();
        using var service = new TelegramNotificationService(store, transport, bus);
        await service.StartAsync(TestContext.Current.CancellationToken);
        try
        {
            bus.Enqueue(new MainCore.Notifications.BuildPlanApplied(new(1), new(2), 3, "NewVillage", "Append", "test-plan"));
            var sent = await transport.Sent.Reader.ReadAsync(TestContext.Current.CancellationToken).AsTask()
                .WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            sent.Chat.ShouldBe("42");
            sent.Text.ShouldContain("3 upgrades");
            sent.Text.ShouldContain("Goal: NewVillage");
        }
        finally { await service.StopAsync(TestContext.Current.CancellationToken); }
    }

    [Fact]
    public async Task EventBusHonorsCheckboxesDisabledAccountsAndDuplicateSuppression()
    {
        var store = new Store(); store.Options[1] = Enabled();
        var transport = new Transport(); var bus = Bus();
        using var service = new TelegramNotificationService(store, transport, bus);
        await service.StartAsync(TestContext.Current.CancellationToken);
        try
        {
            bus.Enqueue(Alert()); bus.Enqueue(Alert());
            bus.Enqueue(Alert() with { Kind = TelegramEvents.Errors });
            bus.Enqueue(Alert(2));
            var sent = await transport.Sent.Reader.ReadAsync(TestContext.Current.CancellationToken).AsTask().WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            sent.Chat.ShouldBe("42");
            sent.Text.ShouldContain("Account 1");
            await Task.Delay(3500, TestContext.Current.CancellationToken);
            transport.Calls.ShouldBe(1);
        }
        finally { await service.StopAsync(TestContext.Current.CancellationToken); }
    }

    [Fact]
    public async Task ChangingDestinationDropsQueuedMessagesFromTheOldConfiguration()
    {
        var store = new Store(); store.Options[1] = Enabled();
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var transport = new Transport { OnSend = () => release.Task };
        using var service = new TelegramNotificationService(store, transport, Bus());
        await service.StartAsync(TestContext.Current.CancellationToken);
        try
        {
            service.Publish(Alert()); service.Publish(Alert(key: "second"));
            await transport.Sent.Reader.ReadAsync(TestContext.Current.CancellationToken).AsTask().WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            service.SaveSettings(new(1), Enabled("99"), "fake");
            release.SetResult();
            await Task.Delay(3500, TestContext.Current.CancellationToken);
            transport.Calls.ShouldBe(1);
            service.Publish(Alert(key: "third"));
            var next = await transport.Sent.Reader.ReadAsync(TestContext.Current.CancellationToken).AsTask().WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            next.Chat.ShouldBe("99");
        }
        finally { release.TrySetResult(); await service.StopAsync(TestContext.Current.CancellationToken); }
    }

    [Fact]
    public async Task FailedDeliveryIsVisibleAndDoesNotStopSubsequentAlerts()
    {
        var store = new Store(); store.Options[1] = Enabled();
        var transport = new Transport { OnSend = () => throw new TelegramDeliveryException("Telegram rejected the token.") };
        using var service = new TelegramNotificationService(store, transport, Bus());
        await service.StartAsync(TestContext.Current.CancellationToken);
        try
        {
            service.Publish(Alert());
            await Until(() => service.DeliveryStatus(new(1)).Contains("rejected"));
            transport.Calls.ShouldBe(1); // Network/authentication failures aren't retried blindly.
            transport.OnSend = null;
            service.Publish(Alert(key: "next"));
            await Until(() => service.DeliveryStatus(new(1)).Contains("delivered"));
            transport.Calls.ShouldBe(2);
        }
        finally { await service.StopAsync(TestContext.Current.CancellationToken); }
    }

    [Fact]
    public async Task RateLimitedDeliveryRetriesOnceAndRechecksEnabledSettings()
    {
        var store = new Store(); store.Options[1] = Enabled();
        var transport = new Transport { OnSend = () => throw new TelegramDeliveryException("Rate limited", 1) };
        using var service = new TelegramNotificationService(store, transport, Bus());
        await service.StartAsync(TestContext.Current.CancellationToken);
        try
        {
            service.Publish(Alert());
            await transport.Sent.Reader.ReadAsync(TestContext.Current.CancellationToken).AsTask().WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            service.SaveSettings(new(1), new(), "");
            await Task.Delay(3500, TestContext.Current.CancellationToken);
            transport.Calls.ShouldBe(1);
        }
        finally { await service.StopAsync(TestContext.Current.CancellationToken); }
    }
}
