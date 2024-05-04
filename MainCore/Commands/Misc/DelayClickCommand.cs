namespace MainCore.Commands.Misc
{
    [RegisterAsCommand]
    public class DelayClickCommand
    {
        private readonly IAccountSettingRepository _accountSettingRepository;

        public DelayClickCommand(IAccountSettingRepository accountSettingRepository)
        {
            _accountSettingRepository = accountSettingRepository;
        }

        public async Task Execute(AccountId accountId)
        {
            var delay = _accountSettingRepository.GetByName(accountId, AccountSettingEnums.ClickDelayMin, AccountSettingEnums.ClickDelayMax);
            await Task.Delay(delay, CancellationToken.None);
        }
    }
}