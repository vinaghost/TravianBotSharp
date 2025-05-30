﻿using MainCore.Constraints;
using MainCore.UI.ViewModels.Tabs;

namespace MainCore.Notifications.Handlers.Refresh
{
    [Handler]
    public static partial class AccountSettingRefresh
    {
        private static async ValueTask HandleAsync(
            IAccountConstraint notification,
            AccountSettingViewModel viewModel,
            CancellationToken cancellationToken
        )
        {
            await viewModel.SettingRefresh(notification.AccountId);
        }
    }
}