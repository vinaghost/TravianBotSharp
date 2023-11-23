using MainCore.UI.ViewModels.UserControls;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Windows;

namespace WPFUI.Views.UserControls
{
    public class ResourceInputUcBase : ReactiveUserControl<ResourceInputViewModel>
    {
    }

    /// <summary>
    /// Interaction logic for ResourceInputUc.xaml
    /// </summary>
    public partial class ResourceInputUc : ResourceInputUcBase
    {
        public ResourceInputUc()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.Bind(ViewModel, vm => vm.Wood, v => v.Wood.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.Clay, v => v.Clay.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.Iron, v => v.Iron.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.Crop, v => v.Crop.Text).DisposeWith(d);
            });
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(ResourceInputUc), new PropertyMetadata(default(string)));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
    }
}