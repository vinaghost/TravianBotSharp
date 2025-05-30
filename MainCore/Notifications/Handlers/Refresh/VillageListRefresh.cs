﻿using MainCore.Constraints;
using MainCore.UI.ViewModels.Tabs;

namespace MainCore.Notifications.Handlers.Refresh
{
    [Handler]
    public static partial class VillageListRefresh
    {
        private static async ValueTask HandleAsync(
            IAccountConstraint notification,
            VillageViewModel viewModel,
            CancellationToken cancellationToken)
        {
            await viewModel.VillageListRefresh(notification.AccountId);
        }
    }
}