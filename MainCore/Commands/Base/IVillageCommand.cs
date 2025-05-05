namespace MainCore.Commands.Base
{
    public interface IVillageCommand : IAccountCommand
    {
        VillageId VillageId { get; }
    }
}