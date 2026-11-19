namespace MainCore.Shared.Errors;

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

public class UpgradeError(UpgradeErrorType type) : Error
{
    public UpgradeErrorType Type => type;
}

public sealed class UpgradeErrorNextExecute(UpgradeErrorType type, DateTime nextExecute) : UpgradeError(type)
{
    public DateTime NextExecute { get; } = nextExecute;
}