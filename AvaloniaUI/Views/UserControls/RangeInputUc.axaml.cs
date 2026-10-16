using Avalonia;

using MainCore.UI.ViewModels.UserControls;

using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.Primitives;

namespace AvaloniaUI.Views.UserControls
{
    public class RangeInputUcBase : ReactiveUserControl<RangeInputViewModel>
    {
    }

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

        public static readonly StyledProperty<string?> TextProperty =
            AvaloniaProperty.Register<RangeInputUc, string?>(nameof(Text));

        public string? Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly StyledProperty<string?> UnitProperty =
            AvaloniaProperty.Register<RangeInputUc, string?>(nameof(Unit));

        public string? Unit
        {
            get => GetValue(UnitProperty);
            set => SetValue(UnitProperty, value);
        }
    }
}
