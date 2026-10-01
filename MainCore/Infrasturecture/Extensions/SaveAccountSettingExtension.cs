using System;
using System.Collections.Generic;
using System.Text;

namespace MainCore.Infrasturecture.Extensions
{
    public static class SaveAccountSettingExtension
    {
        public static void SaveAccountSetting(this AppDbContext context, AccountId accountId, Dictionary<AccountSettingEnums, int> settings)
        {
            if (settings.Count == 0) return;
            foreach (var setting in settings)
            {
                context.AccountsSetting
                    .Where(x => x.AccountId == accountId.Value)
                    .Where(x => x.Setting == setting.Key)
                    .ExecuteUpdate(x => x.SetProperty(x => x.Value, setting.Value));
            }
        }
    }
}