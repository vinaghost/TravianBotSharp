using MainCore.Commands.General;
using MainCore.Commands.Navigate;
using MainCore.Commands.Step.TrainTroop;
using MainCore.Commands.Update;
using MainCore.Commands.Validate;

namespace MainCore.Commands
{
    public interface IUnitOfCommand
    {
        IDelayClickCommand DelayClickCommand { get; }
        IDelayTaskCommand DelayTaskCommand { get; }
        ISwitchTabCommand SwitchTabCommand { get; }
        ISwitchVillageCommand SwitchVillageCommand { get; }
        IToBuildingCommand ToBuildingCommand { get; }
        IToDorfCommand ToDorfCommand { get; }
        IToHeroInventoryCommand ToHeroInventoryCommand { get; }
        IUpdateAccountInfoCommand UpdateAccountInfoCommand { get; }
        IUpdateDorfCommand UpdateDorfCommand { get; }
        IUpdateFarmListCommand UpdateFarmListCommand { get; }
        IUpdateHeroItemsCommand UpdateHeroItemsCommand { get; }
        IUpdateVillageListCommand UpdateVillageListCommand { get; }
        IGetMaximumTroopCommand GetMaximumTroopCommand { get; }
        IInputAmountTroopCommand InputAmountTroopCommand { get; }
        IValidateProxyCommand ValidateProxyCommand { get; }
    }
}