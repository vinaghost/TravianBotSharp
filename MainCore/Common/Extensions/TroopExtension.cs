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

        public static long[] GetTrainCost(this TroopEnums troop)
        {
            return troop switch
            {
                TroopEnums.Legionnaire => new long[] { 120, 100, 150, 30 },
                TroopEnums.Praetorian => new long[] { 100, 130, 160, 70 },
                TroopEnums.Imperian => new long[] { 150, 160, 210, 80 },
                TroopEnums.EquitesLegati => new long[] { 140, 160, 20, 40 },
                TroopEnums.EquitesImperatoris => new long[] { 550, 440, 320, 100 },
                TroopEnums.EquitesCaesaris => new long[] { 550, 640, 800, 180 },
                TroopEnums.RomanRam => new long[] { 900, 360, 500, 70 },
                TroopEnums.RomanCatapult => new long[] { 950, 1350, 600, 90 },
                TroopEnums.RomanChief => new long[] { 30750, 27200, 45000, 37500 },
                TroopEnums.RomanSettler => new long[] { 4600, 4200, 5800, 4400 },
                TroopEnums.Clubswinger => new long[] { 95, 75, 40, 40 },
                TroopEnums.Spearman => new long[] { 145, 70, 85, 40 },
                TroopEnums.Axeman => new long[] { 130, 120, 170, 70 },
                TroopEnums.Scout => new long[] { 160, 100, 50, 50 },
                TroopEnums.Paladin => new long[] { 370, 270, 290, 75 },
                TroopEnums.TeutonicKnight => new long[] { 450, 515, 480, 80 },
                TroopEnums.TeutonRam => new long[] { 1000, 300, 350, 70 },
                TroopEnums.TeutonCatapult => new long[] { 900, 1200, 600, 60 },
                TroopEnums.TeutonChief => new long[] { 35500, 26600, 25000, 27200 },
                TroopEnums.TeutonSettler => new long[] { 5800, 4400, 4600, 5200 },
                TroopEnums.Phalanx => new long[] { 100, 130, 55, 30 },
                TroopEnums.Swordsman => new long[] { 140, 150, 185, 60 },
                TroopEnums.Pathfinder => new long[] { 170, 150, 20, 40 },
                TroopEnums.TheutatesThunder => new long[] { 350, 450, 230, 60 },
                TroopEnums.Druidrider => new long[] { 360, 330, 280, 120 },
                TroopEnums.Haeduan => new long[] { 500, 620, 675, 170 },
                TroopEnums.GaulRam => new long[] { 950, 555, 330, 75 },
                TroopEnums.GaulCatapult => new long[] { 960, 1450, 630, 90 },
                TroopEnums.GaulChief => new long[] { 30750, 45400, 31000, 37500 },
                TroopEnums.GaulSettler => new long[] { 4400, 5600, 4200, 3900 },
                TroopEnums.SlaveMilitia => new long[] { 45, 60, 30, 15 },
                TroopEnums.AshWarden => new long[] { 115, 100, 145, 60 },
                TroopEnums.KhopeshWarrior => new long[] { 170, 180, 220, 80 },
                TroopEnums.SopduExplorer => new long[] { 170, 150, 20, 40 },
                TroopEnums.AnhurGuard => new long[] { 360, 330, 280, 120 },
                TroopEnums.ReshephChariot => new long[] { 450, 560, 610, 180 },
                TroopEnums.EgyptianRam => new long[] { 995, 575, 340, 80 },
                TroopEnums.EgyptianCatapult => new long[] { 980, 1510, 660, 100 },
                TroopEnums.EgyptianChief => new long[] { 34000, 50000, 34000, 42000 },
                TroopEnums.EgyptianSettler => new long[] { 5040, 6510, 4830, 4620 },
                TroopEnums.Mercenary => new long[] { 130, 80, 40, 40 },
                TroopEnums.Bowman => new long[] { 140, 110, 60, 60 },
                TroopEnums.Spotter => new long[] { 170, 150, 20, 40 },
                TroopEnums.SteppeRider => new long[] { 290, 370, 190, 45 },
                TroopEnums.Marksman => new long[] { 320, 350, 330, 50 },
                TroopEnums.Marauder => new long[] { 450, 560, 610, 140 },
                TroopEnums.HunRam => new long[] { 1060, 330, 360, 70 },
                TroopEnums.HunCatapult => new long[] { 950, 1280, 620, 60 },
                TroopEnums.HunChief => new long[] { 37200, 27600, 25200, 27600 },
                TroopEnums.HunSettler => new long[] { 6100, 4600, 4800, 5400 },
                _ => Array.Empty<long>(),
            };
        }
    }
}