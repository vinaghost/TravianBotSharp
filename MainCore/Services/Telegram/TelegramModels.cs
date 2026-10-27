namespace MainCore.Services.Telegram;

[Flags]
public enum TelegramEvents
{
    None = 0, IncomingAttack = 1, BuildingCompleted = 2, BuildQueueEmpty = 4,
    StorageFull = 8, LowCrop = 16, AccountStatus = 32, Errors = 64, AiPlanApplied = 128,
    All = 255
}

public sealed record TelegramOptions
{
    public bool Enabled { get; init; }
    public string ChatId { get; init; } = "";
    public TelegramEvents Events { get; init; } = TelegramEvents.IncomingAttack | TelegramEvents.LowCrop | TelegramEvents.Errors;
    public bool Silent { get; init; }
    public string Revision { get; init; } = "";

    public void Validate()
    {
        if ((Events & ~TelegramEvents.All) != 0) throw new InvalidOperationException("Select supported notification types.");
        if (Enabled && Events == TelegramEvents.None) throw new InvalidOperationException("Select at least one notification type or disable notifications.");
        if (Enabled) ValidateChat(ChatId);
    }

    public static long ValidateChat(string chatId) => long.TryParse(chatId, out var id) && id != 0
        ? id : throw new InvalidOperationException("Enter a numeric Telegram chat ID or select a chat from Find chats.");
}

public sealed record TelegramChat(string Id, string Name)
{
    public string Display => $"{Name} ({Id})";
}
public sealed record ObservedBuilding(int Slot, BuildingEnums Type, int Level);
public sealed record ObservedStorage(long Wood, long Clay, long Iron, long Crop, long Warehouse, long Granary);
public sealed record TelegramVillageScan(AccountId AccountId, VillageId VillageId, bool? UnderAttack = null,
    ObservedBuilding[]? Buildings = null, ObservedStorage? Storage = null) : IAccountVillageNotification;
public sealed record TelegramAlert(AccountId AccountId, VillageId? VillageId, TelegramEvents Kind, string Text, string Key) : IAccountNotification;

// Uses observed game state, never the wording of log messages, to detect village events.
public sealed class TelegramDetector
{
    private sealed class State
    {
        public bool Attack, Full, Low;
        public Dictionary<int, ObservedBuilding> Buildings { get; } = [];
    }
    private readonly Dictionary<(int, int), State> _villages = [];
    private readonly Dictionary<int, StatusEnums> _statuses = [];
    public void Reset(AccountId accountId)
    {
        foreach (var key in _villages.Keys.Where(x => x.Item1 == accountId.Value).ToArray()) _villages.Remove(key);
        _statuses.Remove(accountId.Value);
    }

    public IEnumerable<TelegramAlert> Observe(TelegramVillageScan scan)
    {
        var key = (scan.AccountId.Value, scan.VillageId.Value);
        if (!_villages.TryGetValue(key, out var state)) _villages[key] = state = new();
        if (scan.UnderAttack is bool attack)
        {
            var newlyDetected = attack && !state.Attack;
            state.Attack = attack;
            if (newlyDetected) yield return Alert(TelegramEvents.IncomingAttack, "Incoming attack detected. Check your village in-game.", Guid.NewGuid().ToString("N"));
        }
        if (scan.Buildings is not null)
        {
            var completed = new List<string>();
            foreach (var building in scan.Buildings.Where(x => x.Level >= 0 && x.Type is not (BuildingEnums.Unknown or BuildingEnums.Site)))
            {
                if (state.Buildings.TryGetValue(building.Slot, out var previous)
                    && (previous.Type == building.Type || previous.Type == BuildingEnums.Site) && building.Level > previous.Level)
                    completed.Add($"{building.Type}: level {building.Level} (slot {building.Slot})");
                state.Buildings[building.Slot] = building;
            }
            foreach (var empty in scan.Buildings.Where(x => x.Type == BuildingEnums.Site))
                state.Buildings[empty.Slot] = empty with { Level = 0 };
            if (completed.Count > 0) yield return Alert(TelegramEvents.BuildingCompleted, "Completed upgrades observed:\n" + string.Join("\n", completed), string.Join("|", completed));
        }
        if (scan.Storage is { Warehouse: > 0, Granary: > 0 } s)
        {
            var fullest = Math.Max(Math.Max(s.Wood, Math.Max(s.Clay, s.Iron)) / (double)s.Warehouse, s.Crop / (double)s.Granary);
            if (fullest >= .90 && !state.Full)
            {
                state.Full = true;
                yield return Alert(TelegramEvents.StorageFull, "At least one resource is at 90% or more of storage capacity.", Guid.NewGuid().ToString("N"));
            }
            else if (fullest <= .80) state.Full = false;
            var crop = s.Crop / (double)s.Granary;
            if (crop <= .10 && !state.Low)
            {
                state.Low = true;
                yield return Alert(TelegramEvents.LowCrop, $"Crop stocks are low: {s.Crop:N0} / {s.Granary:N0} (10% or less). This is a stock warning, not a starvation forecast.", Guid.NewGuid().ToString("N"));
            }
            else if (crop >= .20) state.Low = false;
        }
        TelegramAlert Alert(TelegramEvents kind, string text, string id) => new(scan.AccountId, scan.VillageId, kind, text, id);
    }

    public TelegramAlert? Status(StatusModified status)
    {
        if (status.Status is not (StatusEnums.Online or StatusEnums.Paused or StatusEnums.Offline)) return null;
        var seen = _statuses.TryGetValue(status.AccountId.Value, out var previous);
        _statuses[status.AccountId.Value] = status.Status;
        if (seen && previous == status.Status || !seen && status.Status == StatusEnums.Offline) return null;
        return new(status.AccountId, null, TelegramEvents.AccountStatus, $"Account status: {status.Status}.", Guid.NewGuid().ToString("N"));
    }
}
