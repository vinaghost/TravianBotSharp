
namespace MainCore.UI.ViewModels.UserControls
{
    public interface IWaitingOverlayViewModel
    {
        Task ChangeMessage(string message);
        Task Hide();
        Task Show();
        Task Show(string message);
    }
}
