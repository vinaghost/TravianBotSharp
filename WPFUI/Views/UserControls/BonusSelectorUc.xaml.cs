using MainCore.UI.ViewModels.UserControls;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Windows;

namespace WPFUI.Views.UserControls
{
    public class BonusSelectorUcBase : ReactiveUserControl<BonusSelectorViewModel>
    {
    }

    /// <summary>
    /// Interaction logic for BonusSelectorUc.xaml
    /// </summary>
    public partial class BonusSelectorUc : BonusSelectorUcBase
    {
        public BonusSelectorUc()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.Items, v => v.BonusComboBox.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedItem, v => v.BonusComboBox.SelectedItem).DisposeWith(d);
            });
        }

        public static readonly DependencyProperty TextProperty =
          DependencyProperty.Register("Text", typeof(string), typeof(BonusSelectorUc), new PropertyMetadata(default(string)));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
    }
}