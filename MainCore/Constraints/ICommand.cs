namespace MainCore.Constraints
{
    public interface ICommand : IConstraint;

    public sealed record Command : ICommand;

    public interface IAccountCommand : ICommand, IAccountConstraint;

    public interface IVillageCommand : ICommand, IVillageConstraint;

    public interface IAccountVillageCommand : ICommand, IAccountVillageConstraint;
}