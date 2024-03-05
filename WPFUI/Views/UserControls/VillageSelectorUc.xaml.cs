using MainCore.UI.ViewModels.UserControls;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Windows;

namespace WPFUI.Views.UserControls
{
    public class VillageSelectorUcBase : ReactiveUserControl<VillageSelectorViewModel>
    {
    }

    /// <summary>
    /// Interaction logic for VillageSelectorUc.xaml
    /// </summary>
    public partial class VillageSelectorUc : VillageSelectorUcBase
    {
        public VillageSelectorUc()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.Items, v => v.VillageComboBox.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedItem, v => v.VillageComboBox.SelectedItem).DisposeWith(d);
            });
        }

        public static readonly DependencyProperty TextProperty =
           DependencyProperty.Register("Text", typeof(string), typeof(VillageSelectorUc), new PropertyMetadata(default(string)));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
    }
}