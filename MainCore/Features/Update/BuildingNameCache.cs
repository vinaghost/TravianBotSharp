using System.Collections.Concurrent;

namespace MainCore.Features.Update;

[RegisterSingleton]
public sealed class BuildingNameCache
{
    private readonly ConcurrentDictionary<(int AccountId, int VillageId), Dictionary<int, Parsers.BuildingNameDto[]>> names = new();

    public void Store(AccountId accountId, VillageId villageId, Parsers.BuildingDataDto snapshot)
    {
        var villageDorfMap = names.GetOrAdd((accountId.Value, villageId.Value), _ => []);
        villageDorfMap[snapshot.Dorf] = [.. snapshot.Names];
    }

    public Parsers.BuildingNameDto[] Get(AccountId accountId, VillageId villageId)
    {
        if (names.TryGetValue((accountId.Value, villageId.Value), out var villageDorfMap))
        {
            return [.. villageDorfMap.Values.SelectMany(x => x)];
        }
        return [];
    }
}