namespace MainCore.Shared.Constraints;

public interface ICommand : IConstraint
{
    string Name { get; }
}

public interface IBrowserCommand : ICommand
{
    IChromeBrowser Browser { get; }
}

public interface IVillageBrowserCommand : ICommand
{
    IChromeBrowser Browser { get; }
    VillageId VillageId { get; }
}

public interface IAccountCommand : ICommand, IAccountConstraint;

public interface IAccountVillageCommand : ICommand, IAccountVillageConstraint;

public static class CommandExtensions
{
    public static void Deconstruct(this IBrowserCommand command, out IChromeBrowser browser, out AccountId accountId)
    {
        browser = command.Browser;
        accountId = command.Browser.AccountId;
    }

    public static void Deconstruct(this IVillageBrowserCommand command, out IChromeBrowser browser, out AccountId accountId, out VillageId villageId)
    {
        browser = command.Browser;
        accountId = command.Browser.AccountId;
        villageId = command.VillageId;
    }
}