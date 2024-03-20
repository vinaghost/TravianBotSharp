using MainCore.Common.Enums;
using System.Drawing;

namespace MainCore.Common.Extensions
{
    public static class BuildExtension
    {
        public static bool IsWall(this BuildingEnums building)
        {
            return building switch
            {
                BuildingEnums.EarthWall => true,
                BuildingEnums.CityWall => true,
                BuildingEnums.Palisade => true,
                BuildingEnums.StoneWall => true,
                BuildingEnums.MakeshiftWall => true,
                _ => false,
            };
        }

        public static BuildingEnums GetWall(this TribeEnums tribe)
        {
            return tribe switch
            {
                TribeEnums.Romans => BuildingEnums.CityWall,
                TribeEnums.Teutons => BuildingEnums.EarthWall,
                TribeEnums.Gauls => BuildingEnums.Palisade,
                TribeEnums.Egyptians => BuildingEnums.StoneWall,
                TribeEnums.Huns => BuildingEnums.MakeshiftWall,
                _ => BuildingEnums.Site,
            };
        }

        public static bool IsMultipleBuilding(this BuildingEnums building)
        {
            return building switch
            {
                BuildingEnums.Warehouse => true,
                BuildingEnums.Granary => true,
                BuildingEnums.GreatWarehouse => true,
                BuildingEnums.GreatGranary => true,
                BuildingEnums.Trapper => true,
                BuildingEnums.Cranny => true,
                _ => false,
            };
        }

        public static BuildingEnums GetGreat(this BuildingEnums building)
        {
            return building switch
            {
                BuildingEnums.Warehouse => BuildingEnums.GreatWarehouse,
                BuildingEnums.Granary => BuildingEnums.GreatGranary,
                BuildingEnums.Barracks => BuildingEnums.GreatBarracks,
                BuildingEnums.Stable => BuildingEnums.GreatStable,
                _ => building
            };
        }

        public static int GetMaxLevel(this BuildingEnums building)
        {
            return building switch
            {
                BuildingEnums.Bakery => 5,
                BuildingEnums.Brickyard => 5,
                BuildingEnums.IronFoundry => 5,
                BuildingEnums.GrainMill => 5,
                BuildingEnums.Sawmill => 5,

                BuildingEnums.Cranny => 10,
                _ => 20,
            };
        }

        public static List<PrerequisiteBuilding> GetPrerequisiteBuildings(this BuildingEnums building)
        {
            return building switch
            {
                BuildingEnums.Sawmill => new()
                    {
                        new(BuildingEnums.Woodcutter, 10),
                        new(BuildingEnums.MainBuilding, 5),
                    },
                BuildingEnums.Brickyard => new()
                    {
                        new(BuildingEnums.ClayPit, 10),
                        new(BuildingEnums.MainBuilding, 5),
                    },
                BuildingEnums.IronFoundry => new()
                    {
                        new(BuildingEnums.IronMine, 10),
                        new(BuildingEnums.MainBuilding, 5),
                    },
                BuildingEnums.GrainMill => new()
                    {
                        new(BuildingEnums.Cropland, 5),
                    },
                BuildingEnums.Bakery => new()
                    {
                        new(BuildingEnums.Cropland, 10),
                        new(BuildingEnums.GrainMill, 5),
                        new(BuildingEnums.MainBuilding, 5),
                    },
                BuildingEnums.Warehouse => new()
                    {
                        new(BuildingEnums.MainBuilding, 1),
                    },
                BuildingEnums.Granary => new()
                    {
                        new(BuildingEnums.MainBuilding, 1),
                    },
                BuildingEnums.Smithy => new()
                    {
                        new(BuildingEnums.MainBuilding, 3),
                        new(BuildingEnums.Academy, 1),
                    },
                BuildingEnums.TournamentSquare => new()
                    {
                        new(BuildingEnums.RallyPoint, 15),
                    },
                BuildingEnums.Marketplace => new()
                    {
                        new(BuildingEnums.Warehouse, 1),
                        new(BuildingEnums.Granary, 1),
                        new(BuildingEnums.MainBuilding, 3),
                    },
                BuildingEnums.Embassy => new()
                    {
                        new(BuildingEnums.MainBuilding, 1),
                    },
                BuildingEnums.Barracks => new()
                    {
                        new(BuildingEnums.RallyPoint, 1),
                        new(BuildingEnums.MainBuilding, 3),
                    },
                BuildingEnums.Stable => new()
                    {
                        new(BuildingEnums.Academy, 5),
                        new(BuildingEnums.Smithy, 3),
                    },
                BuildingEnums.Workshop => new()
                    {
                        new(BuildingEnums.Academy, 10),
                        new(BuildingEnums.MainBuilding, 5),
                    },
                BuildingEnums.Academy => new()
                    {
                        new(BuildingEnums.Barracks, 3),
                        new(BuildingEnums.MainBuilding, 3),
                    },
                BuildingEnums.TownHall => new()
                    {
                        new(BuildingEnums.Academy, 10),
                        new(BuildingEnums.MainBuilding, 10),
                    },
                BuildingEnums.Residence => new()
                    {
                        new(BuildingEnums.MainBuilding, 5),
                    },
                BuildingEnums.Palace => new()
                    {
                        new(BuildingEnums.MainBuilding, 5),
                        new(BuildingEnums.Embassy, 1),
                    },
                BuildingEnums.Treasury => new()
                    {
                        new(BuildingEnums.MainBuilding, 10),
                    },
                BuildingEnums.TradeOffice => new()
                    {
                        new(BuildingEnums.Stable, 10),
                        new(BuildingEnums.Marketplace, 20),
                    },
                BuildingEnums.GreatBarracks => new()
                    {
                        new(BuildingEnums.Barracks, 20),
                    },
                BuildingEnums.GreatStable => new()
                    {
                        new(BuildingEnums.Stable, 20),
                    },
                BuildingEnums.StonemasonsLodge => new()
                    {
                        new(BuildingEnums.MainBuilding, 5),
                    },
                BuildingEnums.Brewery => new()
                    {
                        new(BuildingEnums.Granary, 20),
                        new(BuildingEnums.RallyPoint, 10),
                    },
                BuildingEnums.Trapper => new()
                    {
                        new(BuildingEnums.RallyPoint, 1),
                    },
                BuildingEnums.HerosMansion => new()
                    {
                        new(BuildingEnums.MainBuilding, 3),
                        new(BuildingEnums.RallyPoint, 1),
                    },
                BuildingEnums.GreatWarehouse => new()
                    {
                        new(BuildingEnums.MainBuilding, 10),
                    },
                BuildingEnums.GreatGranary => new()
                    {
                        new(BuildingEnums.MainBuilding, 10),
                    },
                BuildingEnums.HorseDrinkingTrough => new()
                    {
                        new(BuildingEnums.RallyPoint, 10),
                        new(BuildingEnums.Stable, 20),
                    },
                //no res/palace
                BuildingEnums.CommandCenter => new()
                    {
                        new(BuildingEnums.MainBuilding, 5),
                    },
                BuildingEnums.Waterworks => new()
                    {
                        new(BuildingEnums.HerosMansion, 10),
                    },
                _ => new(),
            };
        }

        public static bool IsResourceField(this BuildingEnums building)
        {
            int buildingInt = (int)building;
            // If id between 1 and 4, it's resource field
            return buildingInt < 5 && buildingInt > 0;
        }

        public static Color GetColor(this BuildingEnums building)
        {
            return building switch
            {
                BuildingEnums.Site => Color.White,
                BuildingEnums.Woodcutter => Color.Lime,
                BuildingEnums.ClayPit => Color.Orange,
                BuildingEnums.IronMine => Color.LightGray,
                BuildingEnums.Cropland => Color.Yellow,
                _ => Color.LightCyan,
            };
        }

        public static long[] GetConstructCost(this BuildingEnums building)
        {
            return building switch
            {
                BuildingEnums.Woodcutter => new long[] { 40, 100, 50, 60 },
                BuildingEnums.ClayPit => new long[] { 80, 40, 80, 50 },
                BuildingEnums.IronMine => new long[] { 100, 80, 30, 60 },
                BuildingEnums.Cropland => new long[] { 70, 90, 70, 20 },
                BuildingEnums.Sawmill => new long[] { 520, 380, 290, 90 },
                BuildingEnums.Brickyard => new long[] { 440, 480, 320, 50 },
                BuildingEnums.IronFoundry => new long[] { 200, 450, 510, 120 },
                BuildingEnums.GrainMill => new long[] { 500, 440, 380, 1240 },
                BuildingEnums.Bakery => new long[] { 1200, 1480, 870, 1600 },
                BuildingEnums.Warehouse => new long[] { 130, 160, 90, 40 },
                BuildingEnums.Granary => new long[] { 80, 100, 70, 20 },
                BuildingEnums.Blacksmith => new long[] { 170, 200, 380, 130 },
                BuildingEnums.Smithy => new long[] { 130, 210, 410, 130 },
                BuildingEnums.TournamentSquare => new long[] { 1750, 2250, 1530, 240 },
                BuildingEnums.MainBuilding => new long[] { 70, 40, 60, 20 },
                BuildingEnums.RallyPoint => new long[] { 110, 160, 90, 70 },
                BuildingEnums.Marketplace => new long[] { 80, 70, 120, 70 },
                BuildingEnums.Embassy => new long[] { 180, 130, 150, 80 },
                BuildingEnums.Barracks => new long[] { 210, 140, 260, 120 },
                BuildingEnums.Stable => new long[] { 260, 140, 220, 100 },
                BuildingEnums.Workshop => new long[] { 460, 510, 600, 320 },
                BuildingEnums.Academy => new long[] { 220, 160, 90, 40 },
                BuildingEnums.Cranny => new long[] { 40, 50, 30, 10 },
                BuildingEnums.TownHall => new long[] { 1250, 1110, 1260, 600 },
                BuildingEnums.Residence => new long[] { 580, 460, 350, 180 },
                BuildingEnums.Palace => new long[] { 550, 800, 750, 250 },
                BuildingEnums.Treasury => new long[] { 2880, 2740, 2580, 990 },
                BuildingEnums.TradeOffice => new long[] { 1400, 1330, 1200, 400 },
                BuildingEnums.GreatBarracks => new long[] { 630, 420, 780, 360 },
                BuildingEnums.GreatStable => new long[] { 780, 420, 660, 300 },
                BuildingEnums.CityWall => new long[] { 70, 90, 170, 70 },
                BuildingEnums.EarthWall => new long[] { 120, 200, 0, 80 },
                BuildingEnums.Palisade => new long[] { 160, 100, 80, 60 },
                BuildingEnums.StonemasonsLodge => new long[] { 155, 130, 125, 70 },
                BuildingEnums.Brewery => new long[] { 1460, 930, 1250, 1740 },
                BuildingEnums.Trapper => new long[] { 100, 100, 100, 100 },
                BuildingEnums.HerosMansion => new long[] { 700, 670, 700, 240 },
                BuildingEnums.GreatWarehouse => new long[] { 650, 800, 450, 200 },
                BuildingEnums.GreatGranary => new long[] { 400, 500, 350, 100 },
                BuildingEnums.WW => new long[] { 66700, 69050, 72200, 13200 },
                BuildingEnums.HorseDrinkingTrough => new long[] { 780, 420, 660, 540 },
                BuildingEnums.StoneWall => new long[] { 110, 160, 70, 60 },
                BuildingEnums.MakeshiftWall => new long[] { 50, 80, 40, 30 },
                BuildingEnums.CommandCenter => new long[] { 1600, 1250, 1050, 200 },
                BuildingEnums.Waterworks => new long[] { 910, 945, 910, 340 },
                BuildingEnums.Hospital => new long[] { 320, 280, 420, 360 },
                _ => new long[] { 0, 0, 0, 0 },
            };
        }

        private static double GetKCost(this BuildingEnums building)
        {
            return building switch
            {
                BuildingEnums.Woodcutter => 1.67,
                BuildingEnums.ClayPit => 1.67,
                BuildingEnums.IronMine => 1.67,
                BuildingEnums.Cropland => 1.67,
                BuildingEnums.Sawmill => 1.8,
                BuildingEnums.Brickyard => 1.8,
                BuildingEnums.IronFoundry => 1.8,
                BuildingEnums.GrainMill => 1.8,
                BuildingEnums.Bakery => 1.8,
                BuildingEnums.Warehouse => 1.28,
                BuildingEnums.Granary => 1.28,
                BuildingEnums.Blacksmith => 1.28,
                BuildingEnums.Smithy => 1.28,
                BuildingEnums.TournamentSquare => 1.28,
                BuildingEnums.MainBuilding => 1.28,
                BuildingEnums.RallyPoint => 1.28,
                BuildingEnums.Marketplace => 1.28,
                BuildingEnums.Embassy => 1.28,
                BuildingEnums.Barracks => 1.28,
                BuildingEnums.Stable => 1.28,
                BuildingEnums.Workshop => 1.28,
                BuildingEnums.Academy => 1.28,
                BuildingEnums.Cranny => 1.28,
                BuildingEnums.TownHall => 1.28,
                BuildingEnums.Residence => 1.28,
                BuildingEnums.Palace => 1.28,
                BuildingEnums.Treasury => 1.26,
                BuildingEnums.TradeOffice => 1.28,
                BuildingEnums.GreatBarracks => 1.28,
                BuildingEnums.GreatStable => 1.28,
                BuildingEnums.CityWall => 1.28,
                BuildingEnums.EarthWall => 1.28,
                BuildingEnums.Palisade => 1.28,
                BuildingEnums.StonemasonsLodge => 1.28,
                BuildingEnums.Brewery => 1.4,
                BuildingEnums.Trapper => 1.28,
                BuildingEnums.HerosMansion => 1.33,
                BuildingEnums.GreatWarehouse => 1.28,
                BuildingEnums.GreatGranary => 1.28,
                BuildingEnums.WW => 1.0275,
                BuildingEnums.HorseDrinkingTrough => 1.28,
                BuildingEnums.StoneWall => 1.28,
                BuildingEnums.MakeshiftWall => 1.28,
                BuildingEnums.CommandCenter => 1.22,
                BuildingEnums.Waterworks => 1.31,
                BuildingEnums.Hospital => 1.28,
                BuildingEnums.Site => 1.28,
                _ => 1,
            };
        }

        public static long[] GetUpgradeCost(this BuildingEnums building, int level)
        {
            var k = building.GetKCost();
            var cost = building.GetConstructCost();

            if (level == 1) return cost;

            return cost
                .Select(x => RoundMul(x * Math.Pow(k, level - 1), 5))
                .ToArray(); ;
        }

        private static long RoundMul(double v, double n)
        {
            return (long)(Math.Round(v / n) * n);
        }
    }
}

public struct PrerequisiteBuilding
{
    public PrerequisiteBuilding(BuildingEnums type, int level)
    {
        Type = type;
        Level = level;
    }

    public BuildingEnums Type { get; set; }
    public int Level { get; set; }
}