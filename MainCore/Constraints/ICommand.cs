namespace MainCore.Constraints
{
    public interface ICommand : IConstraint
    {
        string Name { get; }
    }

    public interface IAccountCommand : ICommand, IAccountConstraint;

    public interface IVillageCommand : ICommand, IVillageConstraint;

    public interface IAccountVillageCommand : ICommand, IAccountVillageConstraint;
}