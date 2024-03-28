using DynamicData;
using MainCore.Common.Enums;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;

namespace MainCore.UI.ViewModels.UserControls
{
    public class TroopSelectorBasedOnTribeViewModel : TroopSelectorViewModel
    {
        public void Set(TribeEnums tribe)
        {
            Items.Clear();
            Items.Add(new(TroopEnums.None));
            var troops = GetTroops(tribe);
            Items.AddRange(troops.Select(x => new TroopItem(x)));
            SelectedItem = Items.First();
        }

        private static List<TroopEnums> GetTroops(TribeEnums tribe)
        {
            return tribe switch
            {
                TribeEnums.Romans => new()
                {
                    TroopEnums.Legionnaire,
                    TroopEnums.Praetorian,
                    TroopEnums.Imperian,
                    TroopEnums.EquitesLegati,
                    TroopEnums.EquitesImperatoris,
                    TroopEnums.EquitesCaesaris,
                    TroopEnums.RomanRam,
                    TroopEnums.RomanCatapult,
                    TroopEnums.RomanChief,
                },
                TribeEnums.Teutons => new()
                {
                    TroopEnums.Clubswinger,
                    TroopEnums.Spearman,
                    TroopEnums.Axeman,
                    TroopEnums.Scout,
                    TroopEnums.Paladin,
                    TroopEnums.TeutonicKnight,
                    TroopEnums.TeutonRam,
                    TroopEnums.TeutonCatapult,
                    TroopEnums.TeutonChief,
                },
                TribeEnums.Gauls => new()
                {
                   TroopEnums.Phalanx,
                    TroopEnums.Swordsman,
                    TroopEnums.Pathfinder,
                    TroopEnums.TheutatesThunder,
                    TroopEnums.Druidrider,
                    TroopEnums.Haeduan,
                    TroopEnums.GaulRam,
                    TroopEnums.GaulCatapult,
                    TroopEnums.GaulChief
                },
                TribeEnums.Nature => new(),
                TribeEnums.Natars => new(),
                TribeEnums.Egyptians => new()
                {
                    TroopEnums.SlaveMilitia,
                    TroopEnums.AshWarden,
                    TroopEnums.KhopeshWarrior,
                    TroopEnums.SopduExplorer,
                    TroopEnums.AnhurGuard,
                    TroopEnums.ReshephChariot,
                    TroopEnums.EgyptianRam,
                    TroopEnums.EgyptianCatapult,
                    TroopEnums.EgyptianChief
                },
                TribeEnums.Huns => new()
                {
                    TroopEnums.Mercenary,
                    TroopEnums.Bowman,
                    TroopEnums.Spotter,
                    TroopEnums.SteppeRider,
                    TroopEnums.Marksman,
                    TroopEnums.Marauder,
                    TroopEnums.HunRam,
                    TroopEnums.HunCatapult,
                    TroopEnums.HunChief
                },
                TribeEnums.Spartan => new()    //REGE
                {
                    TroopEnums.Hoplite,
                    TroopEnums.Sentinel,
                    TroopEnums.Shieldsman,
                    TroopEnums.TwinsteelTherion,
                    TroopEnums.ElpidaRider,
                    TroopEnums.CorinthianCrusher,
                    TroopEnums.SpartanRam,
                    TroopEnums.SpartanBallista,
                    TroopEnums.SpartanChief,
                    TroopEnums.SpartanSettler,
                },
                _ => new(),
            };
        }
    }
}