using MainCore.Common.Enums;

namespace MainCore.Common.Extensions
{
    public static class TroopExtension
    {
        public static TribeEnums GetTribe(this TroopEnums troop)
        {
            return (int)troop switch
            {
                >= (int)TroopEnums.Legionnaire and <= (int)TroopEnums.RomanSettler => TribeEnums.Romans,
                >= (int)TroopEnums.Clubswinger and <= (int)TroopEnums.TeutonSettler => TribeEnums.Teutons,
                >= (int)TroopEnums.Phalanx and <= (int)TroopEnums.GaulSettler => TribeEnums.Gauls,
                >= (int)TroopEnums.Rat and <= (int)TroopEnums.Elephant => TribeEnums.Nature,
                >= (int)TroopEnums.Pikeman and <= (int)TroopEnums.Settler => TribeEnums.Natars,
                >= (int)TroopEnums.SlaveMilitia and <= (int)TroopEnums.EgyptianSettler => TribeEnums.Egyptians,
                >= (int)TroopEnums.Mercenary and <= (int)TroopEnums.HunSettler => TribeEnums.Huns,
                _ => TribeEnums.Any,
            };
        }

        public static BuildingEnums GetTrainBuilding(this TroopEnums troop)
        {
            return troop switch
            {
                TroopEnums.Legionnaire or TroopEnums.Praetorian or TroopEnums.Imperian => BuildingEnums.Barracks,
                TroopEnums.EquitesLegati or TroopEnums.EquitesImperatoris or TroopEnums.EquitesCaesaris => BuildingEnums.Stable,
                TroopEnums.RomanRam or TroopEnums.RomanCatapult => BuildingEnums.Workshop,
                TroopEnums.Clubswinger or TroopEnums.Spearman or TroopEnums.Axeman or TroopEnums.Scout => BuildingEnums.Barracks,
                TroopEnums.Paladin or TroopEnums.TeutonicKnight => BuildingEnums.Stable,
                TroopEnums.TeutonRam or TroopEnums.TeutonCatapult => BuildingEnums.Workshop,
                TroopEnums.Phalanx or TroopEnums.Swordsman => BuildingEnums.Barracks,
                TroopEnums.Pathfinder or TroopEnums.TheutatesThunder or TroopEnums.Druidrider or TroopEnums.Haeduan => BuildingEnums.Stable,
                TroopEnums.GaulRam or TroopEnums.GaulCatapult => BuildingEnums.Workshop,
                TroopEnums.SlaveMilitia or TroopEnums.AshWarden or TroopEnums.KhopeshWarrior => BuildingEnums.Barracks,
                TroopEnums.SopduExplorer or TroopEnums.AnhurGuard or TroopEnums.ReshephChariot => BuildingEnums.Stable,
                TroopEnums.EgyptianRam or TroopEnums.EgyptianCatapult => BuildingEnums.Workshop,
                TroopEnums.Mercenary or TroopEnums.Bowman => BuildingEnums.Barracks,
                TroopEnums.Spotter or TroopEnums.SteppeRider or TroopEnums.Marksman or TroopEnums.Marauder => BuildingEnums.Stable,
                TroopEnums.HunRam or TroopEnums.HunCatapult => BuildingEnums.Workshop,
                _ => BuildingEnums.Site,
            };
        }
    }
}