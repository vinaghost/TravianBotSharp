using MainCore.UI.ViewModels.UserControls;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Windows;

namespace WPFUI.Views.UserControls
{
    public class TribeSelectorUcBase : ReactiveUserControl<TribeSelectorViewModel>
    {
    }

    /// <summary>
    /// Interaction logic for TribeSelectorUc.xaml
    /// </summary>
    public partial class TribeSelectorUc : TribeSelectorUcBase
    {
        public TribeSelectorUc()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.Items, v => v.TribeComboBox.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedItem, v => v.TribeComboBox.SelectedItem).DisposeWith(d);
            });
        }

        public static readonly DependencyProperty TextProperty =
          DependencyProperty.Register("Text", typeof(string), typeof(TribeSelectorUc), new PropertyMetadata(default(string)));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
    }
}
