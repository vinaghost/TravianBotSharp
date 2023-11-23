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
                        new(BuildingEnums.Cropland, 10),
                        new(BuildingEnums.MainBuilding, 5),
                    },
                BuildingEnums.Bakery => new()
                    {
                        new(BuildingEnums.Cropland, 10),
                        new(BuildingEnums.GrainMill, 10),
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
}