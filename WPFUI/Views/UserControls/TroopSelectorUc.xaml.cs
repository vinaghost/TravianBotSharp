using MainCore.UI.ViewModels.UserControls;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Windows;

namespace WPFUI.Views.UserControls
{
    public class TroopSelectorUcBase : ReactiveUserControl<TroopSelectorViewModel>
    {
    }

    /// <summary>
    /// Interaction logic for TroopSelectorUc.xaml
    /// </summary>
    public partial class TroopSelectorUc : TroopSelectorUcBase
    {
        public TroopSelectorUc()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.Items, v => v.TroopComboBox.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedItem, v => v.TroopComboBox.SelectedItem).DisposeWith(d);
            });
        }

        public static readonly DependencyProperty TextProperty =
           DependencyProperty.Register("Text", typeof(string), typeof(TroopSelectorUc), new PropertyMetadata(default(string)));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
    }
}