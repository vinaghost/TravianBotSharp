namespace MainCore.Services
{
    [RegisterSingleton<IDialogService, DialogService>]
    public sealed class DialogService : IDialogService
    {
        public Action<string, string> MessageBoxFunc { get; set; }
        public Func<string, string, bool> ConfirmBoxFunc { get; set; }
        public Func<string> OpenFileDialogFunc { get; set; }
        public Func<string> SaveFileDialogFunc { get; set; }

        public string OpenFileDialog() => OpenFileDialogFunc?.Invoke();

        public string SaveFileDialog() => SaveFileDialogFunc?.Invoke();

        public void ShowMessageBox(string title, string message) => MessageBoxFunc?.Invoke(title, message);

        public bool ShowConfirmBox(string title, string message) => ConfirmBoxFunc?.Invoke(title, message) ?? false;
    }
}