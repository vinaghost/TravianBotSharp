using MainCore.UI.Models.Output;
using ReactiveUI.Primitives;

namespace MainCore.Services
{
    public interface IDialogService
    {
        Interaction<MessageBoxData, bool> ConfirmBox { get; }
        Interaction<MessageBoxData, RxVoid> MessageBox { get; }
        Interaction<RxVoid, string> FileOpenDialog { get; }
        Interaction<RxVoid, string> FileSaveDialog { get; }

        Task<string> OpenFileDialog();

        Task<string> SaveFileDialog();

        Task<bool> SendConfirm(string title, string message);

        Task SendMessage(string title, string message);
    }
}