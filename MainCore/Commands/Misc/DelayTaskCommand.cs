using MainCore.Commands.Abstract;

namespace MainCore.Commands.Misc
{
    [RegisterScoped<DelayTaskCommand>]
    public class DelayTaskCommand : CommandBase, ICommand
    {
        private readonly IGetSetting _getSetting;

        public DelayTaskCommand(IDataService dataService, IGetSetting getSetting) : base(dataService)
        {
            _getSetting = getSetting;
        }

        public async Task<Result> Execute(CancellationToken cancellationToken)
        {
            var accountId = _dataService.AccountId;
            var delay = _getSetting.ByName(accountId, AccountSettingEnums.TaskDelayMin, AccountSettingEnums.TaskDelayMax);

            var result = await Result.Try(() => Task.Delay(delay, cancellationToken), static _ => Cancel.Error);
            return result;
        }
    }
}