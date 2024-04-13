using MainCore.UI.ViewModels.Tabs;
using ReactiveUI;
using System.Reactive.Disposables;

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
                this.BindCommand(ViewModel, vm => vm.Export, v => v.ExportButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.Import, v => v.ImportButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.Save, v => v.SaveButton).DisposeWith(d);

                this.Bind(ViewModel, vm => vm.AccountSettingInput.ClickDelay, v => v.ClickDelay.ViewModel).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccountSettingInput.TaskDelay, v => v.TaskDelay.ViewModel).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccountSettingInput.WorkTime, v => v.WorkTime.ViewModel).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccountSettingInput.SleepTime, v => v.SleepTime.ViewModel).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccountSettingInput.EnableAutoLoadVillage, v => v.EnableAutoLoadVillage.IsChecked).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccountSettingInput.Tribe, v => v.Tribes.ViewModel).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccountSettingInput.HeadlessChrome, v => v.HeadlessChrome.IsChecked).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccountSettingInput.EnableAutoStartAdventure, v => v.EnableAutoStartAdventure.IsChecked).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccountSettingInput.EquipGearBeforeStartAdventure, v => v.EquipGearBeforeStartAdventure.IsChecked).DisposeWith(d);

                this.Bind(ViewModel, vm => vm.AccountSettingInput.EnableAutoSetHeroPoint, v => v.EnableAutoSetHeroPoint.IsChecked).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccountSettingInput.HeroFightingPoint, v => v.HeroFightingPoint.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccountSettingInput.HeroOffPoint, v => v.HeroOffPoint.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccountSettingInput.HeroDefPoint, v => v.HeroDefPoint.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccountSettingInput.HeroResourcePoint, v => v.HeroResourcePoint.Text).DisposeWith(d);

                this.Bind(ViewModel, vm => vm.AccountSettingInput.EnableAutoReviveHero, v => v.EnableAutoReviveHero.IsChecked).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccountSettingInput.UseHeroResourceToRevive, v => v.UseHeroResourceToRevive.IsChecked).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccountSettingInput.HeroRespawnVillage, v => v.HeroRespawnVillage.ViewModel).DisposeWith(d);

                this.Bind(ViewModel, vm => vm.AccountSettingInput.HealingBeforeStartAdventure, v => v.HealingBeforeStartAdventure.IsChecked).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccountSettingInput.HealthBeforeStartAdventure, v => v.HealthBeforeStartAdventure.Text).DisposeWith(d);

                this.Bind(ViewModel, vm => vm.AccountSettingInput.EnableDonateResource, v => v.EnableDonateResource.IsChecked).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccountSettingInput.Bonus, v => v.DonateResourceType.ViewModel).DisposeWith(d);
            });
        }
    }
}