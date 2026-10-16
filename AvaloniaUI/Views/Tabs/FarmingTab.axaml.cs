using Avalonia.Controls;
using Avalonia.Input.Platform;

using MainCore.UI.ViewModels.Tabs;

using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.Primitives;

namespace AvaloniaUI.Views.Tabs
{
    public class FarmingTabBase : ReactiveUserControl<FarmingViewModel>
    {
    }

    public partial class FarmingTab : FarmingTabBase
    {
        public FarmingTab()
        {
            InitializeComponent();
            CopyCoordinatesButton.Click += async (_, _) => await CopyCoordinates();

            this.WhenActivated(d =>
            {
                this.BindCommand(ViewModel, vm => vm.SaveCommand, v => v.SaveButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.StartCommand, v => v.StartButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.StopCommand, v => v.StopButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.ActiveFarmListCommand, v => v.ActiveButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.UpdateFarmListCommand, v => v.Load).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.FarmLists.Items, v => v.FarmlistGrid.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.FarmLists.SelectedItem, v => v.FarmlistGrid.SelectedItem).DisposeWith(d);

                this.Bind(ViewModel, vm => vm.AccountSettingInput.UseStartAllButton, v => v.UseStartAllCheckbox.IsChecked).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.AccountSettingInput.FarmInterval, v => v.FarmInterval.ViewModel).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.ActiveText, v => v.ActiveButton.Content).DisposeWith(d);

                // inactive finder
                this.OneWayBind(ViewModel, vm => vm.CenterVillages.Items, v => v.CenterVillageComboBox.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.CenterVillages.SelectedItem, v => v.CenterVillageComboBox.SelectedItem).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccountSettingInput.InactiveFinderRadius.Value, v => v.InactiveRadius.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.AccountSettingInput.InactiveFinderPopulation, v => v.InactivePopulation.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.AccountSettingInput.InactiveFinderPlayerPopulation, v => v.InactivePlayerPopulation.ViewModel).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccountSettingInput.InactiveFinderIncludeOases, v => v.IncludeOasesCheckBox.IsChecked).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccountSettingInput.InactiveFinderSkipAnimalOases, v => v.SkipAnimalOasesCheckBox.IsChecked).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.FindInactivesCommand, v => v.FindInactivesButton).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.InactiveStatus, v => v.InactiveStatus.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.InactiveVillages, v => v.InactiveGrid.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccountSettingInput.InactiveFinderTroopSlot.Value, v => v.InactiveTroopSlot.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccountSettingInput.InactiveFinderTroopAmount.Value, v => v.InactiveTroopAmount.Text).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.AddInactivesToFarmListCommand, v => v.AddToFarmListButton).DisposeWith(d);
            });
        }

        private async Task CopyCoordinates()
        {
            var villages = ViewModel?.InactiveVillages;
            if (villages is null || villages.Count == 0) return;

            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (clipboard is null) return;

            await clipboard.SetTextAsync(string.Join(Environment.NewLine, villages.Select(x => x.Coordinates)));
        }
    }
}
