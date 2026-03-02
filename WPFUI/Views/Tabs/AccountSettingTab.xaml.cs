using MainCore.UI.ViewModels.Tabs;
using ReactiveUI;
using System.Reactive.Disposables.Fluent;

namespace WPFUI.Views.Tabs
{
    public class AccountSettingTabBase : ReactiveUserControl<AccountSettingViewModel>
    {
    }

    /// <summary>
    /// Interaction logic for AccountSettingTab.xaml
    /// </summary>
    public partial class AccountSettingTab : AccountSettingTabBase
    {
        public AccountSettingTab()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.BindCommand(ViewModel, vm => vm.ExportCommand, v => v.ExportButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.ImportCommand, v => v.ImportButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.SaveCommand, v => v.SaveButton).DisposeWith(d);

                this.Bind(ViewModel, vm => vm.AccountSettingInput.ClickDelay, v => v.ClickDelay.ViewModel).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccountSettingInput.TaskDelay, v => v.TaskDelay.ViewModel).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccountSettingInput.WorkStartHour, v => v.WorkStartHour.ViewModel).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccountSettingInput.WorkStartMinute, v => v.WorkStartMinute.ViewModel).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccountSettingInput.WorkEndHour, v => v.WorkEndHour.ViewModel).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccountSettingInput.WorkEndMinute, v => v.WorkEndMinute.ViewModel).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccountSettingInput.RandomMinute, v => v.RandomMinute.ViewModel).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccountSettingInput.EnableAutoLoadVillage, v => v.EnableAutoLoadVillage.IsChecked).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccountSettingInput.Tribe, v => v.Tribes.ViewModel).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccountSettingInput.HeadlessChrome, v => v.HeadlessChrome.IsChecked).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccountSettingInput.EnableAutoStartAdventure, v => v.EnableAutoStartAdventure.IsChecked).DisposeWith(d);
            });
        }
    }
}