using MainCore.Commands.Base;
using MainCore.Commands.General;
using MainCore.Commands.Validate;

namespace MainCore.Commands
{
    [RegisterAsTransient(withoutInterface: true)]
    public record UnitOfCommand(ICommandHandler<OpenBrowserCommand> OpenBrowserCommand,
                                ICommandHandler<CloseBrowserCommand> CloseBrowserCommand,
                                ICommandHandler<SleepBrowserCommand> SleepBrowserCommand,
                                ICommandHandler<DelayClickCommand> DelayClickCommand,
                                ICommandHandler<DelayTaskCommand> DelayTaskCommand,
                                ICommandHandler<SwitchTabCommand> SwitchTabCommand,
                                ICommandHandler<SwitchVillageCommand> SwitchVillageCommand,
                                ICommandHandler<ToBuildingCommand> ToBuildingCommand,
                                ICommandHandler<ToHeroInventoryCommand> ToHeroInventoryCommand,
                                ICommandHandler<UpdateAccountInfoCommand> UpdateAccountInfoCommand,
                                ICommandHandler<UpdateFarmListCommand> UpdateFarmListCommand,
                                ICommandHandler<UpdateHeroItemsCommand> UpdateHeroItemsCommand,
                                ICommandHandler<UpdateVillageListCommand> UpdateVillageListCommand,
                                ICommandHandler<ValidateIngameCommand, bool> ValidateIngameCommand,
                                ICommandHandler<ValidateLoginCommand, bool> ValidateLoginCommand,
                                ICommandHandler<ValidateProxyCommand, bool> ValidateProxyCommand);
}