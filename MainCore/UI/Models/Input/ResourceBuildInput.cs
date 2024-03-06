using Humanizer;
using MainCore.Common.Enums;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace MainCore.UI.Models.Input
{
    public class ResourceBuildInput : ViewModelBase
    {
        public ResourceBuildInput()
        {
            SelectedPlan = Plans[0];
            Level = 10;
        }

        public (ResourcePlanEnums, int) Get()
        {
            return (SelectedPlan.Content, Level);
        }

        public ObservableCollection<ComboBoxItem<ResourcePlanEnums>> Plans { get; set; } = new()
        {
            new ComboBoxItem<ResourcePlanEnums>(ResourcePlanEnums.AllResources, ResourcePlanEnums.AllResources.Humanize()),
            new ComboBoxItem<ResourcePlanEnums>(ResourcePlanEnums.OnlyCrop, ResourcePlanEnums.OnlyCrop.Humanize()),
            new ComboBoxItem<ResourcePlanEnums>(ResourcePlanEnums.ExcludeCrop, ResourcePlanEnums.ExcludeCrop.Humanize()),
        };

        private ComboBoxItem<ResourcePlanEnums> _selectedPlan;

        public ComboBoxItem<ResourcePlanEnums> SelectedPlan
        {
            get => _selectedPlan;
            set => this.RaiseAndSetIfChanged(ref _selectedPlan, value);
        }

        private int _level;

        public int Level
        {
            get => _level;
            set => this.RaiseAndSetIfChanged(ref _level, value);
        }
    }
}