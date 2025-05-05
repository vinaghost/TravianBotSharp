namespace MainCore.Commands.Base
{
    public interface IAccountCommand : ICommand
    {
        AccountId AccountId { get; }
    }
}