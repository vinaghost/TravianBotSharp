namespace MainCore.Constraints
{
    public interface ICommand : IConstraint
    {
        string Name { get; }
    }

    public interface IAccountCommand : ICommand, IAccountConstraint;

    public interface IAccountVillageCommand : ICommand, IAccountVillageConstraint;
}