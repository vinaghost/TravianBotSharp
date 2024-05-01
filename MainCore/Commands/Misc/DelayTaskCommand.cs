namespace MainCore.Commands.Misc
{
    [RegisterAsTransient(withoutInterface: true)]
    public class DelayTaskCommand
    {
        private readonly IAccountSettingRepository _accountSettingRepository;

        public DelayTaskCommand(IAccountSettingRepository accountSettingRepository)
        {
            _accountSettingRepository = accountSettingRepository;
        }

        public async Task Execute(AccountId accountId)
        {
            var delay = _accountSettingRepository.GetByName(accountId, AccountSettingEnums.TaskDelayMin, AccountSettingEnums.TaskDelayMax);
            await Task.Delay(delay, CancellationToken.None);
        }
    }
}