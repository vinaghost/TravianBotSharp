namespace MainCore.Commands.Misc
{
    public class DelayTaskCommand
    {
        public async Task Execute(AccountId accountId)
        {
            var delay = new GetAccountSetting().ByName(accountId, AccountSettingEnums.TaskDelayMin, AccountSettingEnums.TaskDelayMax);
            await Task.Delay(delay, CancellationToken.None);
        }
    }
}