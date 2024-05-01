using MainCore.Commands.General;
using MainCore.Commands.Misc;
using MainCore.Commands.Validate;

namespace MainCore.Commands
{
    [RegisterAsTransient(withoutInterface: true)]
    public record UnitOfCommand(ICommandHandler<OpenBrowserCommand> OpenBrowserCommand,
                                ICommandHandler<CloseBrowserCommand> CloseBrowserCommand,
                                ICommandHandler<SleepBrowserCommand> SleepBrowserCommand,
                                ICommandHandler<DelayTaskCommand> DelayTaskCommand,
                                ICommandHandler<SwitchTabCommand> SwitchTabCommand,
                                ICommandHandler<SwitchVillageCommand> SwitchVillageCommand,
                                ICommandHandler<ToBuildingCommand> ToBuildingCommand,
                                ICommandHandler<UpdateAccountInfoCommand> UpdateAccountInfoCommand,
                                ICommandHandler<UpdateFarmListCommand> UpdateFarmListCommand,
                                ICommandHandler<ValidateProxyCommand, bool> ValidateProxyCommand);
}