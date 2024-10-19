using MainCore.UI.Models.Output;
using ReactiveUI;

namespace MainCore.Services
{
    public interface IDialogService
    {
        Interaction<MessageBoxData, bool> ConfirmBox { get; }
        Interaction<MessageBoxData, Unit> MessageBox { get; }
        Interaction<Unit, string> OpenFileDialog { get; }
        Interaction<Unit, string> SaveFileDialog { get; }
    }
}