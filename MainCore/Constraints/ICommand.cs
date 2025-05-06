namespace MainCore.Constraints
{
    public interface ICommand;

    public interface IAccountCommand : ICommand, IAccountConstraint;

    public interface IVillagetCommand : ICommand, IVillageConstraint;
}