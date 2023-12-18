using MainCore.UI.ViewModels.Tabs.Villages;
using ReactiveUI;
using System.Reactive.Disposables;

namespace WPFUI.Views.Tabs.Villages
{
    public class VillageSettingTabBase : ReactiveUserControl<VillageSettingViewModel>
    {
    }

    /// <summary>
    /// Interaction logic for VillageSettingTab.xaml
    /// </summary>
    public partial class VillageSettingTab : VillageSettingTabBase
    {
        public VillageSettingTab()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.BindCommand(ViewModel, vm => vm.ExportCommand, v => v.ExportButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.ImportCommand, v => v.ImportButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.SaveCommand, v => v.SaveButton).DisposeWith(d);

                this.Bind(ViewModel, vm => vm.VillageSettingInput.UseHeroResourceForBuilding, v => v.UseHeroResForBuilding.IsChecked).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.VillageSettingInput.ApplyRomanQueueLogicWhenBuilding, v => v.UseRomanQueueLogic.IsChecked).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.VillageSettingInput.UseSpecialUpgrade, v => v.UseSpecialUpgrade.IsChecked).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.VillageSettingInput.CompleteImmediately, v => v.CompleteImmediately.IsChecked).DisposeWith(d);

                this.Bind(ViewModel, vm => vm.VillageSettingInput.Tribe, v => v.Tribes.ViewModel).DisposeWith(d);

                this.Bind(ViewModel, vm => vm.VillageSettingInput.TrainTroopEnable, v => v.TrainTroopEnable.IsChecked).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.VillageSettingInput.TrainWhenLowResource, v => v.TrainWhenLowResource.IsChecked).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.VillageSettingInput.TrainTroopRepeatTime, v => v.TrainTroopRepeatTime.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.VillageSettingInput.BarrackTroop, v => v.BarrackTroop.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.VillageSettingInput.BarrackAmount, v => v.BarrackAmount.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.VillageSettingInput.StableTroop, v => v.StableTroop.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.VillageSettingInput.StableAmount, v => v.StableAmount.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.VillageSettingInput.WorkshopTroop, v => v.WorkshopTroop.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.VillageSettingInput.WorkshopAmount, v => v.WorkshopAmount.ViewModel).DisposeWith(d);

                this.Bind(ViewModel, vm => vm.VillageSettingInput.AutoNPCEnable, v => v.AutoNPCEnable.IsChecked).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.VillageSettingInput.AutoNPCOverflow, v => v.AutoNPCOverflow.IsChecked).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.VillageSettingInput.AutoNPCGranaryPercent, v => v.AutoNPCGranaryPercent.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.VillageSettingInput.AutoNPCRatio, v => v.AutoNPCRatio.ViewModel).DisposeWith(d);

                this.Bind(ViewModel, vm => vm.VillageSettingInput.AutoRefreshEnable, v => v.AutoRefreshEnable.IsChecked).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.VillageSettingInput.AutoRefreshTime, v => v.AutoRefreshTime.ViewModel).DisposeWith(d);

                this.Bind(ViewModel, vm => vm.VillageSettingInput.AutoClaimQuestEnable, v => v.AutoClaimQuestEnable.IsChecked).DisposeWith(d);
            });
        }
    }
}