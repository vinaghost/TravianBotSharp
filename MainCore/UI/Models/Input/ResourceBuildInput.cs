using Humanizer;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using System.Collections.ObjectModel;

namespace MainCore.UI.Models.Input
{
    public partial class ResourceBuildInput : ViewModelBase
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

        [Reactive]
        private ComboBoxItem<ResourcePlanEnums> _selectedPlan;

        [Reactive]
        private int _level;
    }
}