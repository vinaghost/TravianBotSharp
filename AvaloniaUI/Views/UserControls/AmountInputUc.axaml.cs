using Avalonia;

using MainCore.UI.ViewModels.UserControls;

using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.Primitives;

namespace AvaloniaUI.Views.UserControls
{
    public class AmountInputUcBase : ReactiveUserControl<AmountInputViewModel>
    {
    }

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

        public static readonly StyledProperty<string?> TextProperty =
            AvaloniaProperty.Register<AmountInputUc, string?>(nameof(Text));

        public string? Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly StyledProperty<string?> UnitProperty =
            AvaloniaProperty.Register<AmountInputUc, string?>(nameof(Unit));

        public string? Unit
        {
            get => GetValue(UnitProperty);
            set => SetValue(UnitProperty, value);
        }
    }
}
