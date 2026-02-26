using MainCore.UI.ViewModels.Tabs.Villages;
using ReactiveUI;
using System;
using System.Reactive.Disposables.Fluent;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WPFUI.Views.Tabs.Villages
{
    public class BuildTabBase : ReactiveUserControl<BuildViewModel>
    {
    }

    /// <summary>
    /// Interaction logic for BuildTab.xaml
    /// </summary>
    public partial class BuildTab : BuildTabBase
    {
        private Point _dragStartPoint;
        private int _dragSourceIndex = -1;

        public BuildTab()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.Buildings.Items, v => v.BuildingsGrid.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.Buildings.SelectedItem, v => v.BuildingsGrid.SelectedItem).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.Buildings.SelectedIndex, v => v.BuildingsGrid.SelectedIndex).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.Jobs.Items, v => v.JobsGrid.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.Jobs.SelectedItem, v => v.JobsGrid.SelectedItem).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.Jobs.SelectedIndex, v => v.JobsGrid.SelectedIndex).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.Queue.Items, v => v.QueueGrid.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.Queue.SelectedItem, v => v.QueueGrid.SelectedItem).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.Queue.SelectedIndex, v => v.QueueGrid.SelectedIndex).DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.UpgradeOneLevelCommand, v => v.UpgradeOneLevelButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.UpgradeMaxLevelCommand, v => v.UpgradeMaxLevelButton).DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.ImportCommand, v => v.ImportButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.ExportCommand, v => v.ExportButton).DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.UpCommand, v => v.UpButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.DownCommand, v => v.DownButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.TopCommand, v => v.TopButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.BottomCommand, v => v.BottomButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.DeleteCommand, v => v.DeleteButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.DeleteAllCommand, v => v.DeleteAllButton).DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.BuildNormalCommand, v => v.NormalBuild).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.NormalBuildInput.Buildings, v => v.NormalBuildings.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.NormalBuildInput.SelectedBuilding, v => v.NormalBuildings.SelectedItem).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.NormalBuildInput.Level, v => v.NormalLevel.Text).DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.BuildResourceCommand, v => v.ResourceBuild).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.ResourceBuildInput.Plans, v => v.ResType.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.ResourceBuildInput.SelectedPlan, v => v.ResType.SelectedItem).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.ResourceBuildInput.Level, v => v.ResourceLevel.Text).DisposeWith(d);
            });
        }

        private void JobsGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(JobsGrid);
            _dragSourceIndex = GetItemIndexAt(_dragStartPoint);
        }

        private void JobsGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            if (_dragSourceIndex < 0) return;

            var currentPoint = e.GetPosition(JobsGrid);
            var movedEnough = Math.Abs(currentPoint.X - _dragStartPoint.X) >= SystemParameters.MinimumHorizontalDragDistance
                || Math.Abs(currentPoint.Y - _dragStartPoint.Y) >= SystemParameters.MinimumVerticalDragDistance;
            if (!movedEnough) return;

            DragDrop.DoDragDrop(JobsGrid, new DataObject(typeof(int), _dragSourceIndex), DragDropEffects.Move);
        }

        private void JobsGrid_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(typeof(int)) ? DragDropEffects.Move : DragDropEffects.None;
            e.Handled = true;
        }

        private async void JobsGrid_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(int))) return;
            if (ViewModel is null) return;

            var sourceIndex = (int)e.Data.GetData(typeof(int))!;
            var targetIndex = GetItemIndexAt(e.GetPosition(JobsGrid));
            if (targetIndex < 0)
            {
                if (JobsGrid.Items.Count == 0) return;
                targetIndex = JobsGrid.Items.Count - 1;
            }

            await ViewModel.ReorderJobs(sourceIndex, targetIndex);
            _dragSourceIndex = -1;
        }

        private int GetItemIndexAt(Point point)
        {
            var element = JobsGrid.InputHitTest(point) as DependencyObject;
            while (element is not null && element is not ListBoxItem)
            {
                element = VisualTreeHelper.GetParent(element);
            }

            if (element is not ListBoxItem item) return -1;
            return JobsGrid.ItemContainerGenerator.IndexFromContainer(item);
        }
    }
}
