using MainCore.UI.ViewModels.UserControls;
using ReactiveUI;
using System.Reactive.Disposables.Fluent;
using System.Windows;

namespace WPFUI.Views.UserControls
{
    public class MinuteInputUcBase : ReactiveUserControl<MinuteInputViewModel>
    {
    }

    /// <summary>
    /// Interaction logic for MinuteInputUc.xaml
    /// </summary>
    public partial class MinuteInputUc : MinuteInputUcBase
    {
        public MinuteInputUc()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.Bind(ViewModel, vm => vm.Minute, v => v.MinuteValue.Text).DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.IncreaseMinuteCommand, v => v.IncreaseButton)
                    .DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.DecreaseMinuteCommand, v => v.DecreaseButton)
                    .DisposeWith(d);
            });
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(MinuteInputUc), new PropertyMetadata(default(string)));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
    }
}