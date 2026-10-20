using Avalonia.Controls;

namespace AvaloniaUI.Views
{
    /// <summary>
    /// Replacement for WPF's MessageBox: an OK box, or a Yes/No box when isConfirm is true.
    /// </summary>
    public partial class MessageDialog : Window
    {
        private bool _result;

        public MessageDialog()
        {
            InitializeComponent();
        }

        private MessageDialog(string title, string message, bool isConfirm) : this()
        {
            Title = title;
            MessageText.Text = message;

            YesButton.IsVisible = isConfirm;
            NoButton.IsVisible = isConfirm;
            OkButton.IsVisible = !isConfirm;

            // hidden buttons still react to Enter/Escape, so only arm the visible ones
            YesButton.IsDefault = isConfirm;
            NoButton.IsCancel = isConfirm;
            OkButton.IsDefault = !isConfirm;
            OkButton.IsCancel = !isConfirm;

            YesButton.Click += (_, _) => CloseWith(true);
            NoButton.Click += (_, _) => CloseWith(false);
            OkButton.Click += (_, _) => CloseWith(false);
        }

        private void CloseWith(bool result)
        {
            _result = result;
            Close(result);
        }

        public static async Task<bool> Show(string title, string message, bool isConfirm)
        {
            var dialog = new MessageDialog(title, message, isConfirm);

            var owner = App.GetMainWindow();
            if (owner is not null && owner.IsVisible)
            {
                return await dialog.ShowDialog<bool>(owner);
            }

            var closed = new TaskCompletionSource<bool>();
            dialog.Closed += (_, _) => closed.TrySetResult(dialog._result);
            dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            dialog.Show();
            return await closed.Task;
        }
    }
}
