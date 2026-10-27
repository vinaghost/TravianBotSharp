using System.Security.Cryptography;
using System.Text;
using MainCore.Commands.Navigate;
using MainCore.Commands.Update;
using Microsoft.Extensions.DependencyInjection;

namespace MainCore.Services.AiPlanning
{
    public sealed class PlannerSnapshotService(IDbContextFactory<AppDbContext> factory, CustomServiceScopeFactory scopes, TaskManager tasks)
    {
        public void EnsurePaused(AccountId accountId)
        {
            if (tasks.GetStatus(accountId) is not (StatusEnums.Paused or StatusEnums.Offline) || tasks.IsExecuting(accountId))
                throw new InvalidOperationException("Pause this account and wait for its current task to finish before using the planner.");
        }

        public async Task<VillageSnapshot> RefreshAsync(AccountId accountId, VillageId villageId, CancellationToken token)
        {
            EnsurePaused(accountId);
            using var scope = scopes.CreateScope(accountId);
            var browser = scope.ServiceProvider.GetRequiredService<IChromeBrowser>();
            if (!browser.IsInitialized || !await LoginParser.IsIngamePage(browser.CurrentPage))
                throw new InvalidOperationException("Log in with the bot, then pause the account before analyzing the village.");
            using (var db = factory.CreateDbContext())
                if (!db.Villages.Any(x => x.Id == villageId.Value && x.AccountId == accountId.Value))
                    throw new InvalidOperationException("The selected village does not belong to this account.");
            token.ThrowIfCancellationRequested();
            Check(await scope.ServiceProvider.GetRequiredService<SwitchVillageCommand.Handler>().HandleAsync(new(villageId), token));
            var navigate = scope.ServiceProvider.GetRequiredService<ToDorfCommand.Handler>();
            var update = scope.ServiceProvider.GetRequiredService<UpdateBuildingCommand.Handler>();
            Check(await navigate.HandleAsync(new(1), token));
            // Force a fresh read even when navigation was already on dorf1.
            Check(await browser.Refresh());
            var fields = await BuildingLayoutParser.GetFields(browser.CurrentPage);
            if (fields.Count != 18 || fields.Any(x => x.Location is < 1 or > 18 || !x.Type.IsResourceField() || x.Level < 0))
                throw new InvalidOperationException("Could not read the current resource fields. Check the bot browser and try again.");
            Check(await update.HandleAsync(new(villageId), token));
            var production = await ReadProductionAsync(browser.CurrentPage);
            Check(await navigate.HandleAsync(new(2), token));
            var center = await BuildingLayoutParser.GetInfrastructures(browser.CurrentPage);
            if (center.Count < 20 || center.Any(x => x.Location is < 19 or > 40 || x.Type == BuildingEnums.Unknown))
                throw new InvalidOperationException("Could not read the current village center. Check the bot browser and try again.");
            Check(await update.HandleAsync(new(villageId), token));
            await scope.ServiceProvider.GetRequiredService<UpdateAccountInfoCommand.Handler>().HandleAsync(new(accountId), token);
            await scope.ServiceProvider.GetRequiredService<UpdateStorageCommand.Handler>().HandleAsync(new(accountId, villageId), token);
            var underAttack = await ReadAttackStatusAsync(browser.CurrentPage, villageId);
            EnsurePaused(accountId);
            token.ThrowIfCancellationRequested();
            using var context = factory.CreateDbContext();
            context.Villages.Where(x => x.Id == villageId.Value && x.AccountId == accountId.Value)
                .ExecuteUpdate(x => x.SetProperty(v => v.IsUnderAttack, underAttack));
            return Read(context, accountId, villageId, production);
        }

        public static async Task<bool> ReadAttackStatusAsync(IPage page, VillageId villageId)
        {
            var node = VillagePanelParser.GetVillageNode(page, villageId);
            if (await node.CountAsync() != 1 || !await VillagePanelParser.IsActive(node))
                throw new InvalidOperationException("The browser is no longer showing the selected village. Select it again before planning.");
            return await node.EvaluateAsync<bool>("node => node.classList.contains('attack')");
        }

        private static void Check(Result result)
        {
            if (result.IsFailed) throw new InvalidOperationException("Could not refresh the village. Check the bot browser and try again.");
        }

        public static async Task<long[]?> ReadProductionAsync(IPage page)
        {
            var values = await page.EvaluateAsync<string[]>("""
                () => Array.from(document.querySelectorAll('#production tr'))
                    .map(row => row.querySelector('td.num')?.textContent?.trim())
                    .filter(value => value != null)
                """);
            if (values.Length != 4) return null;
            var rates = new List<long>();
            foreach (var value in values)
            {
                var normalized = new string(value.Replace('−', '-').Where(c => char.IsDigit(c) || c == '-').ToArray());
                if (!long.TryParse(normalized, out var rate)) return null;
                rates.Add(rate);
            }
            return [.. rates];
        }

        public static VillageSnapshot Read(AppDbContext db, AccountId accountId, VillageId villageId, long[]? production = null)
        {
            var village = db.Villages.AsNoTracking().SingleOrDefault(x => x.Id == villageId.Value && x.AccountId == accountId.Value)
                ?? throw new InvalidOperationException("The village is no longer available for this account.");
            var buildings = db.Buildings.AsNoTracking().Where(x => x.VillageId == villageId.Value).OrderBy(x => x.Location)
                .Select(x => new PlannerBuilding(x.Location, x.Type, x.Level)).ToArray();
            if (buildings.Length < 38 || buildings.Count(x => x.Type.IsResourceField()) != 18 || buildings.Select(x => x.Slot).Distinct().Count() != buildings.Length)
                throw new InvalidOperationException("Village buildings are incomplete. Refresh both fields and village center before planning.");
            var storage = db.Storages.AsNoTracking().SingleOrDefault(x => x.VillageId == villageId.Value)
                ?? throw new InvalidOperationException("Resource data is missing. Refresh the village first.");
            var info = db.AccountsInfo.AsNoTracking().SingleOrDefault(x => x.AccountId == accountId.Value);
            var tribe = (TribeEnums)(db.VillagesSetting.Where(x => x.VillageId == villageId.Value && x.Setting == VillageSettingEnums.Tribe).Select(x => (int?)x.Value).FirstOrDefault() ?? (int)(info?.Tribe ?? TribeEnums.Any));
            if (tribe is TribeEnums.Any or TribeEnums.Nature or TribeEnums.Natars || !Enum.IsDefined(tribe))
                throw new InvalidOperationException("A supported player tribe could not be identified.");
            var roman = db.VillagesSetting.Any(x => x.VillageId == villageId.Value && x.Setting == VillageSettingEnums.ApplyRomanQueueLogicWhenBuilding && x.Value != 0);
            var construction = db.QueueBuildings.AsNoTracking().Where(x => x.VillageId == villageId.Value).OrderBy(x => x.Position).ThenBy(x => x.Id)
                .Select(x => new PlannerConstruction(x.Location, x.Type, x.Level, x.CompleteTime)).ToArray();
            var jobs = db.Jobs.AsNoTracking().Where(x => x.VillageId == villageId.Value).OrderBy(x => x.Position).ThenBy(x => x.Id)
                .Select(x => new PlannerJob(x.Id, x.Position, x.Type, x.Content)).ToArray();
            var resources = new PlannerResources(storage.Wood, storage.Clay, storage.Iron, storage.Crop, storage.Warehouse, storage.Granary, storage.FreeCrop);
            var executionSettings = db.VillagesSetting.AsNoTracking().Where(x => x.VillageId == villageId.Value).OrderBy(x => x.Setting).AsEnumerable()
                .Where(x => x.Setting is VillageSettingEnums.UseHeroResourceForBuilding or VillageSettingEnums.UseSpecialUpgrade
                    or VillageSettingEnums.CompleteImmediately or VillageSettingEnums.AutoNPCEnable or VillageSettingEnums.TrainTroopEnable)
                .ToDictionary(x => x.Setting.ToString(), x => x.Value);
            // The timer is sampled in whole seconds. Re-reading it must not make an unchanged queue stale.
            var signature = PlannerJson.Write(new { accountId = accountId.Value, villageId = villageId.Value, buildings,
                construction = construction.Select(x => new { x.Slot, x.Type, x.Level }), jobs, tribe, roman, village.IsUnderAttack, plus = info?.HasPlusAccount ?? false, storage.Warehouse, storage.Granary, executionSettings });
            var revision = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(signature)));
            return new(accountId.Value, villageId.Value, DateTimeOffset.UtcNow, tribe, village.IsUnderAttack, info?.HasPlusAccount ?? false, roman, buildings, construction, jobs, resources, production, revision, executionSettings);
        }
    }
}
