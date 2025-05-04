namespace MainCore.Commands.Base
{
    public interface ICommand;

    public interface IAccountCommand : ICommand
    {
        AccountId AccountId { get; }
    }

    public interface IVillageCommand : IAccountCommand
    {
        VillageId VillageId { get; }
    }
}