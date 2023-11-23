using MainCore.UI.ViewModels.UserControls;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Windows;

namespace WPFUI.Views.UserControls
{
    public partial class AmountInputUcBase : ReactiveUserControl<AmountInputViewModel>
    {
    }

    /// <summary>
    /// Interaction logic for AmountInputUc.xaml
    /// </summary>
    public partial class AmountInputUc : AmountInputUcBase
    {
        public AmountInputUc()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.Bind(ViewModel, vm => vm.Value, v => v.Value.Text).DisposeWith(d);
            });
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(AmountInputUc), new PropertyMetadata(default(string)));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register("Unit", typeof(string), typeof(AmountInputUc), new PropertyMetadata(default(string)));

        public string Unit
        {
            get => (string)GetValue(UnitProperty);
            set => SetValue(UnitProperty, value);
        }
    }
}