namespace MainCore.Features.Update;

public static class BuildingSnapshotReconciler
{
    public static bool ResolveQueue(Parsers.BuildingDataDto snapshot, IEnumerable<Parsers.BuildingNameDto> names)
    {
        var nameTypes = names
            .Where(x => !string.IsNullOrWhiteSpace(x.Name) && Enum.IsDefined(typeof(BuildingEnums), x.Type) && x.Type > 0)
            .GroupBy(x => Normalize(x.Name))
            .ToDictionary(g => g.Key, g => g.Select(x => x.Type).Distinct().ToArray());

        foreach (var item in snapshot.Queue)
        {
            var name = Normalize(item.Name);
            if (nameTypes.TryGetValue(name, out var types))
            {
                item.Type = types.Length == 1 ? types[0] : -1;
            }
            else
            {
                var candidates = Enum.GetValues<BuildingEnums>()
                    .Where(x => (int)x > 0 && Normalize(x.ToString()) == name)
                    .ToArray();
                item.Type = candidates.Length == 1 ? (int)candidates[0] : -1;
            }
        }

        return snapshot.Queue.All(x => x.Type > 0 && x.Level > 0 && x.DurationSeconds >= 0 && x.CompleteTime != default);
    }

    public static bool SameQueue(Parsers.BuildingDataDto first, Parsers.BuildingDataDto second)
    {
        return first.Queue.Length == second.Queue.Length && first.Queue.Zip(second.Queue)
            .All(pair => Normalize(pair.First.Name) == Normalize(pair.Second.Name) && pair.First.Level == pair.Second.Level);
    }

    public static bool IsValidPage(Parsers.BuildingDataDto snapshot)
    {
        return snapshot.Dorf is 1 or 2 && snapshot.Buildings.Length > 0
            && (snapshot.Dorf != 1 || snapshot.Buildings.Length == 18)
            && snapshot.Buildings.All(x => x.Location > 0 && (snapshot.Dorf == 1
                ? x.Location <= 18 && ((BuildingEnums)x.Type).IsResourceField() && x.Level >= 0
                : x.Location > 18 && Enum.IsDefined(typeof(BuildingEnums), x.Type)))
            && snapshot.Buildings.Select(x => x.Location).Distinct().Count() == snapshot.Buildings.Length;
    }

    public static void Apply(AppDbContext context, VillageId villageId, IReadOnlyCollection<Parsers.BuildingDataDto> snapshots, Parsers.BuildingDataDto queueSnapshot)
    {
        var dbBuildings = context.Buildings.Where(x => x.VillageId == villageId.Value).ToList();
        var buildingsByLocation = dbBuildings.ToDictionary(x => x.Location);
        var observed = new Dictionary<int, BuildingDto>();
        foreach (var dto in snapshots.SelectMany(x => x.Buildings))
        {
            observed[dto.Location] = dto;
            if (!buildingsByLocation.TryGetValue(dto.Location, out var building))
            {
                building = new Building { VillageId = villageId.Value, Location = dto.Location };
                context.Buildings.Add(building);
                buildingsByLocation.Add(dto.Location, building);
            }
            building.Type = (BuildingEnums)dto.Type;
            building.Level = dto.Level;
            building.IsUnderConstruction = dto.IsUnderConstruction;
        }

        var existing = context.QueueBuildings.Where(x => x.VillageId == villageId.Value).ToList();
        var unmatched = new List<QueueBuilding>(existing);
        var current = new List<QueueBuilding>();
        foreach (var dto in queueSnapshot.Queue)
        {
            var type = (BuildingEnums)dto.Type;
            var item = unmatched.Where(x => x.Type == type && x.Level == dto.Level)
                .OrderBy(x => Math.Abs((long)x.Position - dto.Position)).FirstOrDefault();
            if (item is null)
            {
                item = new QueueBuilding { VillageId = villageId.Value, Type = type, Level = dto.Level };
                context.QueueBuildings.Add(item);
            }
            else
            {
                unmatched.Remove(item);
            }
            item.Position = dto.Position;
            item.CompleteTime = dto.CompleteTime;
            current.Add(item);
        }
        context.QueueBuildings.RemoveRange(unmatched);

        var candidatesByItem = current.ToDictionary(item => item, item => observed.Values
            .Where(x => x.IsUnderConstruction
                && (item.Type.IsResourceField() ? x.Location <= 18 : x.Location > 18)
                && (x.Type == (int)item.Type || (x.Type == (int)BuildingEnums.Site && x.Level <= 0))
                && x.Level == item.Level - 1)
            .Select(x => x.Location).ToArray());

        foreach (var group in current.GroupBy(x => new { x.Type, x.Level }))
        {
            var items = group.OrderBy(x => x.Position).ToList();
            var pageObserved = snapshots.Any(x => x.Dorf == (group.Key.Type.IsResourceField() ? 1 : 2));
            if (!pageObserved)
            {
                if (items.Count > 1) items.ForEach(x => x.Location = -1);
                continue;
            }

            var candidates = candidatesByItem[items[0]].OrderBy(x => x).ToArray();
            var groupItems = items.ToHashSet();
            var overlapsOtherGroup = candidatesByItem.Any(x => !groupItems.Contains(x.Key) && x.Value.Intersect(candidates).Any());
            if (candidates.Length != items.Count || overlapsOtherGroup)
            {
                items.ForEach(x => x.Location = -1);
                continue;
            }

            for (var i = 0; i < items.Count; i++)
                items[i].Location = candidates[i];
        }

        context.SaveChanges();
    }

    private static string Normalize(string value) => new(value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
}
