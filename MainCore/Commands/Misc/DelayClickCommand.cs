using MainCore.Commands.Abstract;

namespace MainCore.Commands.Misc
{
    [RegisterScoped<DelayClickCommand>]
    public class DelayClickCommand : CommandBase, ICommand
    {
        private readonly IGetSetting _getSetting;

        public DelayClickCommand(IDataService dataService, IGetSetting getSetting) : base(dataService)
        {
            _getSetting = getSetting;
        }

        public async Task<Result> Execute(CancellationToken cancellationToken)
        {
            var accountId = _dataService.AccountId;
            var delay = _getSetting.ByName(accountId, AccountSettingEnums.ClickDelayMin, AccountSettingEnums.ClickDelayMax);

            var result = await Result.Try(() => Task.Delay(delay, cancellationToken), static _ => Cancel.Error);
            return result;
        }
    }
}