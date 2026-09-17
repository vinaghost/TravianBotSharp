using MainCore.UI.Models.Output;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Extensions;

namespace MainCore.Services
{
    [RegisterSingleton<IDialogService, DialogService>]
    public sealed class DialogService : IDialogService
    {
        public Interaction<MessageBoxData, bool> ConfirmBox { get; }
        public Interaction<MessageBoxData, RxVoid> MessageBox { get; }
        public Interaction<RxVoid, string> FileOpenDialog { get; }
        public Interaction<RxVoid, string> FileSaveDialog { get; }

        public DialogService()
        {
            ConfirmBox = new Interaction<MessageBoxData, bool>();
            MessageBox = new Interaction<MessageBoxData, RxVoid>();
            FileOpenDialog = new Interaction<RxVoid, string>();
            FileSaveDialog = new Interaction<RxVoid, string>();
        }

        public async Task SendMessage(string title, string message)
        {
            var messageBoxData = new MessageBoxData(title, message);
            await MessageBox.Handle(messageBoxData).ToHotTask();
        }

        public async Task<bool> SendConfirm(string title, string message)
        {
            var messageBoxData = new MessageBoxData(title, message);
            return await ConfirmBox.Handle(messageBoxData).ToHotTask();
        }

        public async Task<string> OpenFileDialog()
        {
            return await FileOpenDialog.Handle(RxVoid.Default).ToHotTask();
        }

        public async Task<string> SaveFileDialog()
        {
            return await FileSaveDialog.Handle(RxVoid.Default).ToHotTask();
        }
    }
}