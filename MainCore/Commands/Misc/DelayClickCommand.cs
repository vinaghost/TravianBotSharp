using MainCore.Commands.Abstract;

namespace MainCore.Commands.Misc
{
    [RegisterScoped(Registration = RegistrationStrategy.Self)]
    public class DelayClickCommand(DataService dataService) : CommandBase(dataService), ICommand
    {
        public async Task<Result> Execute(CancellationToken cancellationToken)
        {
            var accountId = _dataService.AccountId;
            var delay = new GetSetting().ByName(accountId, AccountSettingEnums.ClickDelayMin, AccountSettingEnums.ClickDelayMax);

            var result = await Result.Try(() => Task.Delay(delay, cancellationToken), static _ => Cancel.Error);
            return result;
        }
    }
}