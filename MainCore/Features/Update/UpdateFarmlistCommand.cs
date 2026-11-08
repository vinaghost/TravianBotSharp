namespace MainCore.Features.Update;

[Handler]
public sealed partial class UpdateFarmlistCommand(
    ChromeManager chromeManager,
    IDbContextFactory<AppDbContext> contextFactory,
    RxQueue rxQueue)
{
    public sealed record Command(AccountId AccountId) : IAccountCommand
    {
        public string Name => "";
    }

    private async ValueTask<Result> HandleAsync(Command command)
    {
        var browser = chromeManager.Get(command.AccountId);
        var dtos = await FarmListParser.GetFarmInfo(browser.CurrentPage);
        UpdateToDatabase(command.AccountId, dtos);
        rxQueue.Enqueue(new FarmsModified(command.AccountId));
        return Result.Ok();
    }

    private void UpdateToDatabase(AccountId accountId, IEnumerable<FarmDto> dtos)
    {
        using var context = contextFactory.CreateDbContext();

        var dtoIds = dtos.Select(x => x.Id.Value).ToHashSet();

        var existingFarms = context.FarmLists
            .Where(x => x.AccountId == accountId.Value)
            .ToList();

        var farmDeleted = existingFarms.Where(x => !dtoIds.Contains(x.Id)).ToList();
        var farmUpdated = existingFarms.Where(x => dtoIds.Contains(x.Id)).ToList();

        var dtoMap = dtos.ToDictionary(x => x.Id.Value);

        if (farmDeleted.Count != 0)
        {
            context.RemoveRange(farmDeleted);
        }

        foreach (var farm in farmUpdated)
        {
            if (dtoMap.TryGetValue(farm.Id, out var dto))
            {
                farm.Name = dto.Name;
            }
        }

        var existingFarmIds = existingFarms.Select(x => x.Id).ToHashSet();
        var farmInserted = dtos
            .Where(x => !existingFarmIds.Contains(x.Id.Value))
            .Select(x => new Farm
            {
                Id = x.Id.Value,
                AccountId = accountId.Value,
                Name = x.Name,
                IsActive = x.IsActive
            })
            .ToList();

        if (farmInserted.Count != 0)
        {
            context.AddRange(farmInserted);
        }

        context.SaveChanges();
    }
}