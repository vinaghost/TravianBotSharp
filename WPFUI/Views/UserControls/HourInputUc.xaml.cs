using MainCore.UI.ViewModels.UserControls;
using ReactiveUI;
using System.Reactive.Disposables.Fluent;
using System.Windows;

namespace WPFUI.Views.UserControls
{
    public class HourInputUcBase : ReactiveUserControl<HourInputViewModel>
    {
    }

    /// <summary>
    /// Interaction logic for HourInputUc.xaml
    /// </summary>
    public partial class HourInputUc : HourInputUcBase
    {
        public HourInputUc()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.Bind(ViewModel, vm => vm.Hour, v => v.HourValue.Text).DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.IncreaseHourCommand, v => v.IncreaseButton)
                    .DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.DecreaseHourCommand, v => v.DecreaseButton)
                    .DisposeWith(d);
            });
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(HourInputUc), new PropertyMetadata(default(string)));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
    }
}
