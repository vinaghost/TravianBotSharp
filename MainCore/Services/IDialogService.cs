namespace MainCore.Services
{
    public interface IDialogService
    {
        string OpenFileDialog();

        string SaveFileDialog();

        bool ShowConfirmBox(string title, string message);

        void ShowMessageBox(string title, string message);
    }
}