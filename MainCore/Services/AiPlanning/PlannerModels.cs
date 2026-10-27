using System.Text.Json;
using System.Text.Json.Serialization;

namespace MainCore.Services.AiPlanning
{
    public enum AiProvider { OpenRouter, OpenAI }
    public enum PlannerGoal { NewVillage, OffensiveArmy, DefensiveArmy, Conqueror, Production }
    public enum QueueApplyMode { Append, ReplacePending }

    public sealed record PlannerOptions
    {
        public AiProvider Provider { get; init; } = AiProvider.OpenRouter;
        public string Model { get; init; } = "openrouter/free";
        public bool FreeOnly { get; init; } = true;
        public PlannerGoal Goal { get; init; } = PlannerGoal.NewVillage;
        public QueueApplyMode ApplyMode { get; init; }
        public int MaxSteps { get; init; } = 25;
        public int HorizonHours { get; init; } = 24;
        public int ServerSpeed { get; init; } = 1;
        public bool? IsCapital { get; init; }
        public string Situation { get; init; } = "";

        public void Validate()
        {
            if (!Enum.IsDefined(Provider) || !Enum.IsDefined(Goal) || !Enum.IsDefined(ApplyMode))
                throw new InvalidOperationException("Select a provider, goal and queue mode.");
            if (string.IsNullOrWhiteSpace(Model) || Model.Length > 150)
                throw new InvalidOperationException("Select a model.");
            if (MaxSteps is < 1 or > 50 || HorizonHours is < 1 or > 168 || ServerSpeed is < 1 or > 100)
                throw new InvalidOperationException("Use 1–50 upgrades, a 1–168 hour horizon, and a server speed of 1–100.");
            if (Situation.Length > 4000) throw new InvalidOperationException("Keep additional context under 4,000 characters.");
        }
    }

    public sealed record AiModel(string Id, bool IsFree, bool SupportsSchema, bool SupportsJson);
    public sealed record PlannerBuilding(int Slot, BuildingEnums Type, int Level);
    public sealed record PlannerConstruction(int Slot, BuildingEnums Type, int Level, DateTime CompleteTime);
    public sealed record PlannerJob(int Id, int Position, JobTypeEnums Type, string Content);
    public sealed record PlannerResources(long Wood, long Clay, long Iron, long Crop, long Warehouse, long Granary, long FreeCrop)
    {
        public long[] Stock => [Wood, Clay, Iron, Crop];
    }
    public sealed record VillageSnapshot(
        int AccountId, int VillageId, DateTimeOffset CapturedAt, TribeEnums Tribe, bool UnderAttack,
        bool HasPlus, bool RomanQueue, PlannerBuilding[] Buildings, PlannerConstruction[] Construction,
        PlannerJob[] PendingJobs, PlannerResources Resources, long[]? ProductionPerHour, string Revision,
        Dictionary<string, int>? ExecutionSettings = null);

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record PlanStep(int BuildingId, int Slot, int TargetLevel, string Reason);
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record AiProposal(string Assessment, PlanStep[] Steps, string[] FollowUpActions, string[] Assumptions);
    public sealed record GuidePassage(string Title, string Url, string Text, string Status);
    public sealed record AiPlanResponse(AiProposal Proposal, string Model, int InputTokens, int OutputTokens);
    public sealed record PreviewStep(int Order, int Slot, int BuildingId, string Building, int FromLevel,
        int TargetLevel, string Cost, string Reason);
    public sealed record PlanValidation(PreviewStep[] Steps, string[] Errors, string[] Warnings, long[] TotalCost)
    {
        public bool IsValid => Errors.Length == 0;
        public bool CanApply => IsValid && Steps.Length > 0;
    }
    public sealed record PlannerPreview(string PlanId, VillageSnapshot Snapshot, PlannerOptions Options,
        AiPlanResponse Response, PlanValidation Validation, GuidePassage[] Sources);

    public static class PlannerJson
    {
        public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
        {
            NumberHandling = JsonNumberHandling.Strict,
            MaxDepth = 32
        };
        public static string Write<T>(T value) => JsonSerializer.Serialize(value, Options);
    }
}
