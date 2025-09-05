#pragma warning disable S1172

namespace MainCore.Commands.Features.TrainTroop
{
    [Handler]
    public static partial class TrainTroopCommand
    {
        public sealed record Command(VillageId VillageId, BuildingEnums Building) : IVillageCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            AppDbContext context,
            IBrowser browser,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var (villageId, building) = command;
            var troop = (TroopEnums)context.ByName(villageId, TroopSettings[building]);
            var maxAmount = TrainTroopParser.GetMaxAmount(browser.Html, troop);
            if (maxAmount == 0)
            {
                return MissingResource.Error(troop);
            }

            var (minSetting, maxSetting) = AmountSettings[building];
            var amount = context.ByName(villageId, minSetting, maxSetting);
            if (amount > maxAmount)
            {
                var trainWhenLowResource = context.BooleanByName(villageId, VillageSettingEnums.TrainWhenLowResource);
                if (!trainWhenLowResource)
                {
                    return MissingResource.Error(troop);
                }
            }

            var result = await TrainTroop(browser, troop, amount, logger);
            if (result.IsFailed) return result;

            logger.Information("Troop training for {Troop} with amount {Amount} is done.", troop, amount);
            return Result.Ok();
        }

        public static Dictionary<BuildingEnums, VillageSettingEnums> TroopSettings { get; } = new()
        {
            {BuildingEnums.Barracks, VillageSettingEnums.BarrackTroop },
            {BuildingEnums.Stable, VillageSettingEnums.StableTroop },
            {BuildingEnums.GreatBarracks, VillageSettingEnums.GreatBarrackTroop },
            {BuildingEnums.GreatStable, VillageSettingEnums.GreatStableTroop },
            {BuildingEnums.Workshop, VillageSettingEnums.WorkshopTroop },
        };

        public static Dictionary<BuildingEnums, (VillageSettingEnums, VillageSettingEnums)> AmountSettings { get; } = new()
        {
            {BuildingEnums.Barracks, (VillageSettingEnums.BarrackAmountMin,VillageSettingEnums.BarrackAmountMax ) },
            {BuildingEnums.Stable, (VillageSettingEnums.StableAmountMin,VillageSettingEnums.StableAmountMax ) },
            {BuildingEnums.GreatBarracks, (VillageSettingEnums.GreatBarrackAmountMin,VillageSettingEnums.GreatBarrackAmountMax ) },
            {BuildingEnums.GreatStable, (VillageSettingEnums.GreatStableAmountMin,VillageSettingEnums.GreatStableAmountMax ) },
            {BuildingEnums.Workshop, (VillageSettingEnums.WorkshopAmountMin,VillageSettingEnums.WorkshopAmountMax ) },
        };

        private static async ValueTask<Result> TrainTroop(
            IBrowser browser,
            TroopEnums troop,
            long amount,
            ILogger logger)
        {
            var inputBox = TrainTroopParser.GetInputBox(browser.Html, troop);
            
            // Eğer ana yöntem başarısız olursa, yeni yapıya özel metodu dene
            if (inputBox is null)
            {
                logger.Warning("Primary input box method failed for troop {Troop}, trying new structure method.", troop);
                inputBox = TrainTroopParser.GetInputBoxNewStructure(browser.Html, troop);
            }
            
            // Hala bulunamazsa, alternatif yöntemi dene
            if (inputBox is null)
            {
                logger.Warning("New structure method failed for troop {Troop}, trying alternative method.", troop);
                inputBox = TrainTroopParser.GetInputBoxAlternative(browser.Html, troop);
            }
            
            if (inputBox is null) 
            {
                // Debug bilgisi ekle
                logger.Warning("Input box not found for troop {Troop} with both methods. HTML structure may have changed.", troop);
                return Retry.TextboxNotFound("troop amount input");
            }

            Result result;
            
            // Önce XPath ile dene
            result = await browser.Input(By.XPath(inputBox.XPath), $"{amount}");
            if (result.IsFailed) 
            {
                logger.Warning("Failed to input amount {Amount} for troop {Troop} with XPath: {XPath}. Trying CSS selector.", amount, troop, inputBox.XPath);
                
                // XPath başarısız olursa, yeni yapıya göre CSS selector ile dene
                var troopIndex = GetTroopIndex(troop);
                if (troopIndex == -1)
                {
                    logger.Warning("Unknown troop type: {Troop}", troop);
                    return result;
                }
                
                // Önce yeni yapıya göre CSS selector dene
                var newCssSelector = $"#nonFavouriteTroops div:nth-child({troopIndex}) input[type='text']";
                result = await browser.Input(By.CssSelector(newCssSelector), $"{amount}");
                
                if (result.IsFailed)
                {
                    // Eğer nonFavouriteTroops'ta bulunamazsa, favouriteTroops'ta dene
                    var favouriteCssSelector = $"#favouriteTroops div:nth-child({troopIndex}) input[type='text']";
                    result = await browser.Input(By.CssSelector(favouriteCssSelector), $"{amount}");
                }
                
                if (result.IsFailed)
                {
                    // Son çare: eski name attribute yöntemi
                    var nameAttribute = $"t{troopIndex}";
                    var fallbackCssSelector = $"input[name='{nameAttribute}']";
                    
                    result = await browser.Input(By.CssSelector(fallbackCssSelector), $"{amount}");
                    if (result.IsFailed)
                    {
                        logger.Warning("Failed to input amount {Amount} for troop {Troop} with all CSS selectors", amount, troop);
                        return result;
                    }
                }
            }

            var trainButton = TrainTroopParser.GetTrainButton(browser.Html);
            if (trainButton is null) 
            {
                logger.Warning("Train button not found. HTML structure may have changed.");
                return Retry.ButtonNotFound("train troop");
            }

            result = await browser.Click(By.XPath(trainButton.XPath));
            if (result.IsFailed) 
            {
                logger.Warning("Failed to click train button. XPath: {XPath}", trainButton.XPath);
                return result;
            }

            return Result.Ok();
        }

        // Troop enum değerini HTML name attribute'una çeviren yardımcı metod
        private static int GetTroopIndex(TroopEnums troop)
        {
            return troop switch
            {
                // Romans
                TroopEnums.Legionnaire => 1,
                TroopEnums.Praetorian => 2,
                TroopEnums.Imperian => 3,
                TroopEnums.EquitesLegati => 4,
                TroopEnums.EquitesImperatoris => 5,
                TroopEnums.EquitesCaesaris => 6,
                TroopEnums.RomanRam => 7,
                TroopEnums.RomanCatapult => 8,
                TroopEnums.RomanChief => 9,
                TroopEnums.RomanSettler => 10,

                // Teutons
                TroopEnums.Clubswinger => 1,
                TroopEnums.Spearman => 2,
                TroopEnums.Axeman => 3,
                TroopEnums.Scout => 4,
                TroopEnums.Paladin => 5,
                TroopEnums.TeutonicKnight => 6,
                TroopEnums.TeutonRam => 7,
                TroopEnums.TeutonCatapult => 8,
                TroopEnums.TeutonChief => 9,
                TroopEnums.TeutonSettler => 10,

                // Gauls
                TroopEnums.Phalanx => 1,
                TroopEnums.Swordsman => 2,
                TroopEnums.Pathfinder => 3,
                TroopEnums.TheutatesThunder => 4,
                TroopEnums.Druidrider => 5,
                TroopEnums.Haeduan => 6,
                TroopEnums.GaulRam => 7,
                TroopEnums.GaulCatapult => 8,
                TroopEnums.GaulChief => 9,
                TroopEnums.GaulSettler => 10,

                // Egyptians
                TroopEnums.SlaveMilitia => 1,
                TroopEnums.AshWarden => 2,
                TroopEnums.KhopeshWarrior => 3,
                TroopEnums.SopduExplorer => 4,
                TroopEnums.AnhurGuard => 5,
                TroopEnums.ReshephChariot => 6,
                TroopEnums.EgyptianRam => 7,
                TroopEnums.EgyptianCatapult => 8,
                TroopEnums.EgyptianChief => 9,
                TroopEnums.EgyptianSettler => 10,

                // Huns
                TroopEnums.Mercenary => 1,
                TroopEnums.Bowman => 2,
                TroopEnums.Spotter => 3,
                TroopEnums.SteppeRider => 4,
                TroopEnums.Marksman => 5,
                TroopEnums.Marauder => 6,
                TroopEnums.HunRam => 7,
                TroopEnums.HunCatapult => 8,
                TroopEnums.HunChief => 9,
                TroopEnums.HunSettler => 10,

                _ => -1
            };
        }
    }
}
