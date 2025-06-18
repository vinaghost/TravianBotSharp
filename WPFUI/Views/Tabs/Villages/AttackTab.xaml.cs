using MainCore.UI.ViewModels.Tabs.Villages;
using ReactiveUI;
using System.Reactive.Disposables;

namespace WPFUI.Views.Tabs.Villages
{
    public class AttackTabBase : ReactiveUserControl<AttackViewModel>
    {
    }

    public partial class AttackTab : AttackTabBase
    {
        public AttackTab()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.Bind(ViewModel,
                        vm => vm.AttackInput.X,
                        v => v.XInput.Text,
                        x => x.ToString(),
                        s => int.TryParse(s, out var value) ? value : 0)
                    .DisposeWith(d);
                this.Bind(ViewModel,
                        vm => vm.AttackInput.Y,
                        v => v.YInput.Text,
                        y => y.ToString(),
                        s => int.TryParse(s, out var value) ? value : 0)
                    .DisposeWith(d);
                this.Bind(ViewModel,
                        vm => vm.AttackInput.ExecuteAt,
                        v => v.TimeInput.Text,
                        dt => dt.ToString("yyyy-MM-dd HH:mm:ss"),
                        s => DateTime.TryParse(s, out var value) ? value : DateTime.Now)
                    .DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AttackInput.AttackType, v => v.AttackType.SelectedValue).DisposeWith(d);

                var troops = ViewModel.AttackInput.Troops;
                T1.ViewModel = troops[0];
                T2.ViewModel = troops[1];
                T3.ViewModel = troops[2];
                T4.ViewModel = troops[3];
                T5.ViewModel = troops[4];
                T6.ViewModel = troops[5];
                T7.ViewModel = troops[6];
                T8.ViewModel = troops[7];
                T9.ViewModel = troops[8];
                T10.ViewModel = troops[9];
                T11.ViewModel = troops[10];

                this.BindCommand(ViewModel, vm => vm.SendCommand, v => v.SendButton).DisposeWith(d);
            });
        }
    }
}
