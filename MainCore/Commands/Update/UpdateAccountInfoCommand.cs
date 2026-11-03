using System.Text.Json;
using System.Text.RegularExpressions;

namespace MainCore.Commands.Update
{
    [Handler]
    public sealed partial class UpdateAccountInfoCommand(
        ChromeManager chromeManager,
        IDbContextFactory<AppDbContext> contextFactory,
        RxQueue rxQueue,
        TaskManager taskManager,
        DefaultTemplatePathStore defaultTemplatePathStore)
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand
        {
            public string Name => "";
        }

        private async ValueTask HandleAsync(Command command)
        {
            var accountId = command.AccountId;
            var browser = chromeManager.Get(accountId);

            var (accountInfo, villageDtos) = await GetData(browser.CurrentPage);

            UpdateAccountInfo(accountId, accountInfo);

            if (villageDtos.Count > 0)
            {
                UpdateVillageList(accountId, villageDtos);
                rxQueue.Enqueue(new VillagesModified(accountId));
            }
        }

        private void UpdateAccountInfo(AccountId accountId, AccountInfoDto accountInfo)
        {
            using var context = contextFactory.CreateDbContext();
            var dbAccountInfo = context.AccountsInfo
                           .FirstOrDefault(x => x.AccountId == accountId.Value);

            if (dbAccountInfo is null)
            {
                dbAccountInfo = new AccountInfo()
                {
                    AccountId = accountId.Value,
                    Gold = accountInfo.Gold,
                    Silver = accountInfo.Silver,
                    HasPlusAccount = accountInfo.HasPlusAccount,
                    Tribe = accountInfo.Tribe
                };
                context.Add(dbAccountInfo);
            }
            else
            {
                dbAccountInfo.Gold = accountInfo.Gold;
                dbAccountInfo.Silver = accountInfo.Silver;
                dbAccountInfo.HasPlusAccount = accountInfo.HasPlusAccount;
                dbAccountInfo.Tribe = accountInfo.Tribe;
            }
            context.SaveChanges();
        }

        private void UpdateVillageList(AccountId accountId, List<VillageDto> dtos)
        {
            using var context = contextFactory.CreateDbContext();

            var dtoIds = dtos.Select(x => x.Id.Value).ToList();

            context.Villages
                .Where(x => x.AccountId == accountId.Value && !dtoIds.Contains(x.Id))
                .ExecuteDelete();

            var villagesToUpdate = context.Villages
                .Where(x => x.AccountId == accountId.Value && dtoIds.Contains(x.Id))
                .ToList();

            foreach (var village in villagesToUpdate)
            {
                var dto = dtos.First(x => x.Id.Value == village.Id);

                village.Name = dto.Name;
                village.X = dto.X;
                village.Y = dto.Y;
                village.IsActive = dto.IsActive;
                village.IsUnderAttack = dto.IsUnderAttack;
            }

            var existingIds = villagesToUpdate.Select(v => v.Id).ToHashSet();
            var newDtos = dtos.Where(x => !existingIds.Contains(x.Id.Value)).ToList();

            foreach (var dto in newDtos)
            {
                var newVillage = new Village()
                {
                    Id = dto.Id.Value,
                    AccountId = accountId.Value,
                    Name = dto.Name,
                    X = dto.X,
                    Y = dto.Y,
                    IsActive = dto.IsActive,
                    IsUnderAttack = dto.IsUnderAttack
                };

                context.Villages.Add(newVillage);
            }

            context.SaveChanges();

            if (newDtos.Count > 0)
            {
                var templateSettings = GetVillageSettingTemplate();
                foreach (var villageId in newDtos.Select(x => x.Id))
                {
                    context.FillVillageSettings(accountId, villageId);
                }

                context.SaveChanges();

                foreach (var villageId in newDtos.Select(x => x.Id))
                {
                    context.SaveVillageSetting(villageId, templateSettings);
                }

                var enableAutoLoadVillageBuilding = context.BooleanByName(accountId, AccountSettingEnums.EnableAutoLoadVillageBuilding);
                if (enableAutoLoadVillageBuilding)
                {
                    foreach (var villageId in newDtos.Select(x => x.Id))
                    {
                        if (taskManager.IsExist<UpdateBuildingTask.Task>(accountId, villageId)) continue;
                        taskManager.AddOrUpdate<UpdateBuildingTask.Task>(new(accountId, villageId));
                    }
                }
            }
        }

        private Dictionary<VillageSettingEnums, int> GetVillageSettingTemplate()
        {
            var path = defaultTemplatePathStore.Get().VillageSettingsPath;
            if (string.IsNullOrWhiteSpace(path)) return [];
            if (!File.Exists(path)) return [];
            try
            {
                var jsonString = File.ReadAllText(path);
                var settings = JsonSerializer.Deserialize<Dictionary<VillageSettingEnums, int>>(jsonString);
                if (settings is null) return [];
                settings.Remove(VillageSettingEnums.Tribe);
                return settings;
            }
            catch
            {
                return [];
            }
        }

        public record struct RawVillageDto(string? IdStr, string? Name, string? CoordinateX, string? CoordinateY, bool Active, bool UnderAttack);
        public record struct RawAccountInfoDto(string? GoldText, string? SilverText, string? PlusClassAttr, string? TribeClassAttr);
        public record struct RawCombinedDataDto(RawAccountInfoDto? AccountInfo, List<RawVillageDto>? Villages);

        [GeneratedRegex(@"vid_(\d+)")]
        private static partial Regex TribeExtractor();

        public static async Task<(AccountInfoDto AccountInfo, List<VillageDto> Villages)> GetData(IPage page)
        {
            var jsonResult = await page.EvaluateAsync<JsonElement>(@"() => {
                const goldEl = document.querySelector('div.ajaxReplaceableGoldAmount');
                const silverEl = document.querySelector('div.ajaxReplaceableSilverAmount');
                const editButton = document.querySelector('#sidebarBoxLinklist a.edit.round');
                const questmasterBtn = document.querySelector('#questmasterButton');

                const accountInfo = {
                    GoldText: goldEl ? (goldEl.innerText || goldEl.textContent).trim() : '0',
                    SilverText: silverEl ? (silverEl.innerText || silverEl.textContent).trim() : '0',
                    PlusClassAttr: editButton ? (editButton.getAttribute('class') || '') : '',
                    TribeClassAttr: questmasterBtn ? (questmasterBtn.getAttribute('class') || '') : ''
                };

                const villageElements = document.querySelectorAll('#sidebarBoxVillageList div.listEntry.village');
                const villages = [];
                villageElements.forEach(node => {
                    const nameEl = node.querySelector('a span.name');
                    const xEl = node.querySelector('span.coordinateX');
                    const yEl = node.querySelector('span.coordinateY');
                    villages.push({
                        IdStr: node.getAttribute('data-did') || '0',
                        Name: nameEl ? (nameEl.innerText || nameEl.textContent).trim() : '',
                        CoordinateX: xEl ? (xEl.innerText || xEl.textContent).trim() : '0',
                        CoordinateY: yEl ? (yEl.innerText || yEl.textContent).trim() : '0',
                        Active: node.classList.contains('active'),
                        UnderAttack: node.classList.contains('attack')
                    });
                });

                return { AccountInfo: accountInfo, Villages: villages };
            }");

            var text = jsonResult.GetRawText();
            var combinedData = JsonSerializer.Deserialize<RawCombinedDataDto>(text);

            // Process Account Info
            var rawAcc = combinedData.AccountInfo ?? new RawAccountInfoDto();
            int gold = (rawAcc.GoldText ?? "").ParseInt();
            int silver = (rawAcc.SilverText ?? "").ParseInt();
            string plusAttr = rawAcc.PlusClassAttr ?? "";
            bool hasPlus = plusAttr.Contains("green");
            string tribeSrc = rawAcc.TribeClassAttr ?? "";
            var tribeMatch = TribeExtractor().Match(tribeSrc);
            TribeEnums tribe = TribeEnums.Any;
            if (tribeMatch.Success)
            {
                int tribeId = int.Parse(tribeMatch.Groups[1].Value);
                tribe = (TribeEnums)tribeId;
            }
            var accountInfoDto = new AccountInfoDto
            {
                Gold = gold,
                Silver = silver,
                HasPlusAccount = hasPlus,
                Tribe = tribe
            };

            var extractedVillages = new List<VillageDto>();
            foreach (var raw in combinedData.Villages ?? [])
            {
                int id = int.TryParse(raw.IdStr, out int parsedId) ? parsedId : 0;
                extractedVillages.Add(new VillageDto
                {
                    Id = new VillageId(id),
                    Name = raw.Name ?? "",
                    X = (raw.CoordinateX ?? "").ParseInt(),
                    Y = (raw.CoordinateY ?? "").ParseInt(),
                    IsActive = raw.Active,
                    IsUnderAttack = raw.UnderAttack
                });
            }

            return (accountInfoDto, extractedVillages);
        }
    }
}