using System.Text.Json;

namespace MainCore.Features.Update;

[Handler]
public sealed partial class UpdateAccountInfoCommand(
    IDbContextFactory<AppDbContext> contextFactory,
    RxQueue rxQueue,
    TaskManager taskManager,
    DefaultTemplatePathStore defaultTemplatePathStore)
{
    public sealed record Command(IChromeBrowser Browser) : IBrowserCommand
    {
        public string Name => "";
    }

    private async ValueTask HandleAsync(Command command)
    {
        var (browser, accountId) = command;
        var (accountInfo, villageDtos) = await browser.CurrentPage.GetAccountData();
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
                Tribe = (TribeEnums)accountInfo.Tribe
            };
            context.Add(dbAccountInfo);
        }
        else
        {
            dbAccountInfo.Gold = accountInfo.Gold;
            dbAccountInfo.Silver = accountInfo.Silver;
            dbAccountInfo.HasPlusAccount = accountInfo.HasPlusAccount;
            dbAccountInfo.Tribe = (TribeEnums)accountInfo.Tribe;
        }
        context.SaveChanges();
    }

    private void UpdateVillageList(AccountId accountId, List<VillageDto> dtos)
    {
        using var context = contextFactory.CreateDbContext();

        var dtoIds = dtos.Select(x => x.Id).ToList();

        context.Villages
            .Where(x => x.AccountId == accountId.Value && !dtoIds.Contains(x.Id))
            .ExecuteDelete();

        var villagesToUpdate = context.Villages
            .Where(x => x.AccountId == accountId.Value && dtoIds.Contains(x.Id))
            .ToList();

        foreach (var village in villagesToUpdate)
        {
            var dto = dtos.First(x => x.Id == village.Id);

            village.Name = dto.Name;
            village.X = dto.X;
            village.Y = dto.Y;
            village.IsActive = dto.IsActive;
            village.IsUnderAttack = dto.IsUnderAttack;
        }

        var existingIds = villagesToUpdate.Select(v => v.Id).ToHashSet();
        var newDtos = dtos.Where(x => !existingIds.Contains(x.Id)).ToList();

        foreach (var dto in newDtos)
        {
            var newVillage = new Village()
            {
                Id = dto.Id,
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
            foreach (var villageId in newDtos.Select(x => new VillageId(x.Id)))
            {
                context.FillVillageSettings(accountId, villageId);
            }

            context.SaveChanges();

            foreach (var villageId in newDtos.Select(x => new VillageId(x.Id)))
            {
                context.SaveVillageSetting(villageId, templateSettings);
            }

            var enableAutoLoadVillageBuilding = context.BooleanByName(accountId, AccountSettingEnums.EnableAutoLoadVillageBuilding);
            if (enableAutoLoadVillageBuilding)
            {
                foreach (var villageId in newDtos.Select(x => new VillageId(x.Id)))
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
}