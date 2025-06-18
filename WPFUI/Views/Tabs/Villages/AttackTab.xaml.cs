using System;
using System.Collections.Generic;
using MainCore.Enums;
using MainCore.Services;
using MainCore.UI.ViewModels.Tabs.Villages;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System.Collections.Immutable;
using System.Reactive.Disposables;

namespace WPFUI.Views.Tabs.Villages
{
    public class AttackTabBase : ReactiveUserControl<AttackViewModel>
    {
    }

    public partial class AttackTab : AttackTabBase
    {
        private static readonly ImmutableDictionary<TribeEnums, string[]> TroopNames = new Dictionary<TribeEnums, string[]>
        {
            {TribeEnums.Romans, new[] {"Legionnaire","Praetorian","Imperian","Equites Legati","Equites Imperatoris","Equites Caesaris","Battering Ram","Fire Catapult","Senator","Settler"}},
            {TribeEnums.Teutons, new[] {"Clubswinger","Spearman","Axeman","Scout","Paladin","Teutonic Knight","Ram","Catapult","Chief","Settler"}},
            {TribeEnums.Gauls, new[] {"Phalanx","Swordsman","Pathfinder","Theutates Thunder","Druidrider","Haeduan","Ram","Trebuchet","Chieftain","Settler"}},
            {TribeEnums.Nature, new[] {"Rat","Spider","Snake","Bat","Wild Boar","Wolf","Bear","Crocodile","Tiger","Elephant"}},
            {TribeEnums.Natars, new[] {"Pikeman","Thorned Warrior","Guardsman","Birds Of Prey","Axerider","Natarian Knight","War Elephant","Ballista","Natarian Emperor","Settler"}},
            {TribeEnums.Egyptians, new[] {"Slave Militia","Ash Warden","Khopesh Warrior","Sopdu Explorer","Anhur Guard","Resheph Chariot","Ram","Stone Catapult","Nomarch","Settler"}},
            {TribeEnums.Huns, new[] {"Mercenary","Bowman","Spotter","Steppe Rider","Marksman","Marauder","Ram","Catapult","Logades","Settler"}},
        }.ToImmutableDictionary();

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
                        vm => vm.AttackInput.ExecuteDate,
                        v => v.DateInput.SelectedDate,
                        d => (DateTime?)d,
                        d => d ?? DateTime.Today)
                    .DisposeWith(d);
                this.Bind(ViewModel,
                        vm => vm.AttackInput.ExecuteTime,
                        v => v.TimeInput.SelectedTime,
                        dt => (DateTime?)dt,
                        dt => dt ?? DateTime.Now)
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

                this.WhenAnyValue(x => x.ViewModel.Tribe)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(UpdateTroopNames)
                    .DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.SendCommand, v => v.SendButton).DisposeWith(d);
            });
        }

        private void UpdateTroopNames(TribeEnums tribe)
        {
            if (!TroopNames.TryGetValue(tribe, out var names)) return;

            T1.Text = names[0];
            T2.Text = names[1];
            T3.Text = names[2];
            T4.Text = names[3];
            T5.Text = names[4];
            T6.Text = names[5];
            T7.Text = names[6];
            T8.Text = names[7];
            T9.Text = names[8];
            T10.Text = names[9];
            T11.Text = "Hero";
        }
    }
}
