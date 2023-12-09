using MainCore.Commands.Base;
using MainCore.Commands.General;
using MainCore.Commands.Navigate;
using MainCore.Commands.Update;
using MainCore.Commands.Validate;
using MainCore.Infrasturecture.AutoRegisterDi;

namespace MainCore.Commands
{
    [RegisterAsTransient(withoutInterface: true)]
    public class UnitOfCommand
    {
        public ICommandHandler<OpenBrowserCommand> OpenBrowserCommand { get; }
        public ICommandHandler<CloseBrowserCommand> CloseBrowserCommand { get; }
        public ICommandHandler<SleepBrowserCommand> SleepBrowserCommand { get; }
        public ICommandHandler<DelayClickCommand> DelayClickCommand { get; }
        public ICommandHandler<DelayTaskCommand> DelayTaskCommand { get; }

        public ICommandHandler<SwitchTabCommand> SwitchTabCommand { get; }
        public ICommandHandler<SwitchVillageCommand> SwitchVillageCommand { get; }
        public ICommandHandler<ToBuildingCommand> ToBuildingCommand { get; }
        public ICommandHandler<ToDorfCommand> ToDorfCommand { get; }
        public ICommandHandler<ToHeroInventoryCommand> ToHeroInventoryCommand { get; }

        public ICommandHandler<UpdateAccountInfoCommand> UpdateAccountInfoCommand { get; }
        public ICommandHandler<UpdateDorfCommand> UpdateDorfCommand { get; }
        public ICommandHandler<UpdateFarmListCommand> UpdateFarmListCommand { get; }
        public ICommandHandler<UpdateHeroItemsCommand> UpdateHeroItemsCommand { get; }
        public ICommandHandler<UpdateVillageListCommand> UpdateVillageListCommand { get; }

        public ICommandHandler<ValidateIngameCommand, bool> ValidateIngameCommand { get; }
        public ICommandHandler<ValidateLoginCommand, bool> ValidateLoginCommand { get; }
        public ICommandHandler<ValidateProxyCommand, bool> ValidateProxyCommand { get; }
    }
}