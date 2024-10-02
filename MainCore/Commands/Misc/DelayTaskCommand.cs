using MainCore.Commands.Abstract;

namespace MainCore.Commands.Misc
{
    [RegisterScoped(Registration = RegistrationStrategy.Self)]
    public class DelayTaskCommand(DataService dataService) : CommandBase(dataService), ICommand
    {
        public async Task<Result> Execute(CancellationToken cancellationToken)
        {
            var accountId = _dataService.AccountId;
            var delay = new GetSetting().ByName(accountId, AccountSettingEnums.TaskDelayMin, AccountSettingEnums.TaskDelayMax);

            var result = await Result.Try(() => Task.Delay(delay, cancellationToken), x => Cancel.Error);
            return result;
        }
    }
}