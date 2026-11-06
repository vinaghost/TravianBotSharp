using System;
using System.Collections.Generic;
using System.Text;

namespace MainCore.Errors
{
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
        public DateTime? NextExecute { get; set; }
    }
}