namespace MainCore.Features.Shared.Errors;

public enum UpgradeErrorType
{
    BuildingNotExist,
    BuildingComplete,
    BuildingInQueue,
    PrerequisiteMissing,
    PrerequisiteInQueue,
    BuildingJobQueueEmpty,
    BuildingJobQueueBroken,
    ConstructionQueueFull,
    LackOfFreeCrop,
    StorageLimit,
    MissingResource
}

public sealed class UpgradeError(UpgradeErrorType type) : Error
{
    public UpgradeErrorType Type => type;
    public DateTime? NextExecute { get; set; }
}