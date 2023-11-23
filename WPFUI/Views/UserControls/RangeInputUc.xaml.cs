using MainCore.UI.ViewModels.UserControls;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Windows;

namespace WPFUI.Views.UserControls
{
    public class RangeInputUcBase : ReactiveUserControl<RangeInputViewModel>
    {
    }

    /// <summary>
    /// Interaction logic for RangeInputUc.xaml
    /// </summary>
    public partial class RangeInputUc : RangeInputUcBase
    {
        public RangeInputUc()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.Bind(ViewModel, vm => vm.Min, v => v.MinValue.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.Max, v => v.MaxValue.Text).DisposeWith(d);
            });
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(RangeInputUc), new PropertyMetadata(default(string)));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register("Unit", typeof(string), typeof(RangeInputUc), new PropertyMetadata(default(string)));

        public string Unit
        {
            get => (string)GetValue(UnitProperty);
            set => SetValue(UnitProperty, value);
        }
    }
}