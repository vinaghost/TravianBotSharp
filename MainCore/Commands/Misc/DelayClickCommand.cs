﻿namespace MainCore.Commands.Misc
{
    public class DelayClickCommand
    {
        public async Task Execute(AccountId accountId)
        {
            var delay = new GetAccountSetting().ByName(accountId, AccountSettingEnums.ClickDelayMin, AccountSettingEnums.ClickDelayMax);
            await Task.Delay(delay, CancellationToken.None);
        }
    }
}