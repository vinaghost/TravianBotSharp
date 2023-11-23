using MainCore.Infrasturecture.AutoRegisterDi;

namespace MainCore.Services
{
    [RegisterAsSingleton]
    public class DialogService : IDialogService
    {
        public Action<string, string> MessageBoxFunc;
        public Func<string, string, bool> ConfirmBoxFunc;
        public Func<string> OpenFileDialogFunc;
        public Func<string> SaveFileDialogFunc;

        public string OpenFileDialog() => OpenFileDialogFunc?.Invoke();

        public string SaveFileDialog() => SaveFileDialogFunc?.Invoke();

        public void ShowMessageBox(string title, string message) => MessageBoxFunc?.Invoke(title, message);

        public bool ShowConfirmBox(string title, string message) => ConfirmBoxFunc?.Invoke(title, message) ?? false;
    }
}