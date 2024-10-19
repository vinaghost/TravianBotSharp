using MainCore.UI.Models.Output;
using ReactiveUI;

namespace MainCore.Services
{
    [RegisterSingleton<IDialogService, DialogService>]
    public sealed class DialogService : IDialogService
    {
        public Interaction<MessageBoxData, bool> ConfirmBox { get; }
        public Interaction<MessageBoxData, Unit> MessageBox { get; }
        public Interaction<Unit, string> OpenFileDialog { get; }
        public Interaction<Unit, string> SaveFileDialog { get; }

        public DialogService()
        {
            ConfirmBox = new Interaction<MessageBoxData, bool>();
            MessageBox = new Interaction<MessageBoxData, Unit>();
            OpenFileDialog = new Interaction<Unit, string>();
            SaveFileDialog = new Interaction<Unit, string>();
        }
    }
}