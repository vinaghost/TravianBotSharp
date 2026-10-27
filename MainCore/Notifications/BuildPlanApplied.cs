namespace MainCore.Notifications
{
    /// <summary>A validated plan has been written to a village's pending build queue.</summary>
    public sealed record BuildPlanApplied(AccountId AccountId, VillageId VillageId,
        int UpgradeCount, string Goal, string QueueMode, string PlanId) : IAccountVillageNotification;
}
