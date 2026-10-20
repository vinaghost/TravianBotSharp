using Avalonia;

using MainCore.UI.ViewModels.UserControls;

using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.Primitives;

namespace AvaloniaUI.Views.UserControls
{
    public class ResourceInputUcBase : ReactiveUserControl<ResourceInputViewModel>
    {
    }

    public partial class ResourceInputUc : ResourceInputUcBase
    {
        public ResourceInputUc()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.Bind(ViewModel, vm => vm.Wood, v => v.Wood.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.Clay, v => v.Clay.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.Iron, v => v.Iron.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.Crop, v => v.Crop.Text).DisposeWith(d);
            });
        }

        public static readonly StyledProperty<string?> TextProperty =
            AvaloniaProperty.Register<ResourceInputUc, string?>(nameof(Text));

        public string? Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
    }
}
