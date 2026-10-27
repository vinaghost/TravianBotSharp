using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MainCore.Entities;
using MainCore.Services.Telegram;

namespace WPFUI.Views
{
    public partial class TelegramSettingsWindow : Window
    {
        private readonly TelegramNotificationService _service;
        private readonly AccountId _accountId;
        private CancellationTokenSource _request;
        public TelegramSettingsWindow(TelegramNotificationService service, AccountId accountId)
        {
            _service = service; _accountId = accountId;
            InitializeComponent();
            try
            {
                AccountText.Text = service.Settings.Describe(accountId, null);
                var options = service.Settings.Load(accountId);
                TokenBox.Password = service.Settings.ReadToken(accountId, options);
                EnabledBox.IsChecked = options.Enabled; ChatIdBox.Text = options.ChatId; SilentBox.IsChecked = options.Silent;
                foreach (var (box, flag) in Choices()) box.IsChecked = (options.Events & flag) != 0;
                StatusText.Text = service.DeliveryStatus(accountId);
            }
            catch { StatusText.Text = "Could not load Telegram settings. Re-enter them and Save, or disable and forget the token."; }
        }
        private (CheckBox, TelegramEvents)[] Choices() => new[]
        {
            (AttackBox, TelegramEvents.IncomingAttack), (BuildingBox, TelegramEvents.BuildingCompleted),
            (QueueBox, TelegramEvents.BuildQueueEmpty), (StorageBox, TelegramEvents.StorageFull),
            (CropBox, TelegramEvents.LowCrop), (StatusBox, TelegramEvents.AccountStatus),
            (ErrorsBox, TelegramEvents.Errors), (AiBox, TelegramEvents.AiPlanApplied)
        };
        private TelegramOptions Options()
        {
            var events = TelegramEvents.None;
            foreach (var (box, flag) in Choices()) if (box.IsChecked == true) events |= flag;
            return new() { Enabled = EnabledBox.IsChecked == true, ChatId = ChatIdBox.Text.Trim(), Events = events, Silent = SilentBox.IsChecked == true };
        }
        private void SaveSettings(object sender, RoutedEventArgs e)
        {
            try
            {
                var options = Options();
                _service.SaveSettings(_accountId, options, TokenBox.Password);
                StatusText.Text = options.Enabled ? "Saved. Selected notifications are enabled for this account." : "Saved. Telegram notifications are disabled.";
            }
            catch (InvalidOperationException ex) { StatusText.Text = ex.Message; }
            catch { StatusText.Text = "Could not save settings. Check local file access and Windows Credential Manager."; }
        }
        private void ForgetToken(object sender, RoutedEventArgs e)
        {
            try
            {
                _service.SaveSettings(_accountId, Options() with { Enabled = false }, "");
                TokenBox.Clear(); EnabledBox.IsChecked = false;
                StatusText.Text = "Notifications disabled and saved token removed. Pending alerts from the previous settings will be skipped.";
            }
            catch (InvalidOperationException ex) { StatusText.Text = ex.Message; }
            catch { StatusText.Text = "Could not disable notifications. Check local settings access and try again."; }
        }
        private async void FindChats(object sender, RoutedEventArgs e) => await RunAsync(async token =>
        {
            var chats = await _service.FindChatsAsync(TokenBox.Password, token);
            ChatsBox.ItemsSource = chats;
            StatusText.Text = chats.Length == 0 ? "No recent chats found. Send /start to your bot, then try again, or enter the numeric chat ID manually."
                : "Choose the intended chat from the list, then Send test to verify delivery.";
        });
        private void ChatSelected(object sender, SelectionChangedEventArgs e)
        {
            if (ChatsBox.SelectedItem is TelegramChat chat) ChatIdBox.Text = chat.Id;
        }
        private async void SendTest(object sender, RoutedEventArgs e) => await RunAsync(async token =>
        {
            await _service.TestAsync(_accountId, TokenBox.Password, ChatIdBox.Text.Trim(), SilentBox.IsChecked == true, token);
            StatusText.Text = "Test delivered. Save your checkbox selections to enable automatic notifications.";
        });
        private void RefreshStatus(object sender, RoutedEventArgs e) => StatusText.Text = _service.DeliveryStatus(_accountId);
        private async Task RunAsync(Func<CancellationToken, Task> action)
        {
            if (_request is not null) return;
            _request = new CancellationTokenSource(TimeSpan.FromMinutes(6));
            SettingsPanel.IsEnabled = TestButton.IsEnabled = SaveButton.IsEnabled = ForgetButton.IsEnabled = false;
            StatusText.Text = "Contacting Telegram…";
            try { await action(_request.Token); }
            catch (OperationCanceledException) { StatusText.Text = "Request cancelled or timed out. If sending, check the chat before retrying."; }
            catch (TelegramDeliveryException ex) { StatusText.Text = ex.Message; }
            catch (InvalidOperationException ex) { StatusText.Text = ex.Message; }
            catch { StatusText.Text = "Telegram request failed. Check the token, connection and destination."; }
            finally
            {
                _request.Dispose(); _request = null;
                SettingsPanel.IsEnabled = TestButton.IsEnabled = SaveButton.IsEnabled = ForgetButton.IsEnabled = true;
            }
        }
        private void OnClosing(object sender, CancelEventArgs e)
        {
            if (_request is null) return;
            _request.Cancel(); e.Cancel = true;
            StatusText.Text = "Cancelling the request. Close this window after it stops.";
        }
    }
}
