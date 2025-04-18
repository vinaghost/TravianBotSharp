namespace MainCore.Notification
{
    public record ByAccountVillageIdBase(AccountId AccountId, VillageId VillageId) : ByAccountIdBase(AccountId);
}