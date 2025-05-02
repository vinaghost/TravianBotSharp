using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

#pragma warning disable CS1591

namespace MainCore;

public static class HandlerServiceCollectionExtensions
{
	public static IServiceCollection AddMainCoreBehaviors(
		this IServiceCollection services)
	{
		
		return services;
	}

	public static IServiceCollection AddMainCoreHandlers(
		this IServiceCollection services,
		ServiceLifetime lifetime = ServiceLifetime.Scoped
	)
	{
		global::MainCore.Commands.Checks.CheckAdventureCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.Checks.CheckQuestCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.Features.ClaimQuest.ClaimQuestCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.Features.ClaimQuest.ToQuestPageCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.Features.CompleteImmediately.CompleteImmediatelyCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.Features.DisableContextualHelp.DisableContextualHelpCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.Features.DisableContextualHelp.ToOptionsPageCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.Features.LoginCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.Features.NpcResource.NpcResourceCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.Features.NpcResource.ToNpcResourcePageCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.Features.SleepCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.Features.StartAdventure.ExploreAdventureCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.Features.StartAdventure.ToAdventurePageCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.Features.StartFarmList.StartActiveFarmListCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.Features.StartFarmList.StartAllFarmListCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.Features.StartFarmList.ToFarmListPageCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.Features.TrainTroop.TrainTroopCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.Features.UpgradeBuilding.HandleJobCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.Features.UpgradeBuilding.HandleResourceCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.Features.UpgradeBuilding.HandleUpgradeCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.Features.UpgradeBuilding.ToBuildPageCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.Features.UseHeroItem.ToHeroInventoryCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.Features.UseHeroItem.UseHeroResourceCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.Misc.AddJobCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.Misc.DelayClickCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.Misc.DelayTaskCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.Misc.DeleteJobByIdCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.Misc.OpenBrowserCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.Navigate.SwitchTabCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.Navigate.SwitchVillageCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.Navigate.ToBuildingCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.Navigate.ToDorfCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.Queries.GetAccessQuery.AddHandlers(services, lifetime);
		global::MainCore.Commands.Queries.GetBuildingLocationQuery.AddHandlers(services, lifetime);
		global::MainCore.Commands.Queries.GetBuildingQuery.AddHandlers(services, lifetime);
		global::MainCore.Commands.Queries.GetHasBuildJobVillagesQuery.AddHandlers(services, lifetime);
		global::MainCore.Commands.Queries.GetLayoutBuildingsQuery.AddHandlers(services, lifetime);
		global::MainCore.Commands.Queries.GetMissingBuildingVillagesQuery.AddHandlers(services, lifetime);
		global::MainCore.Commands.Queries.GetVillageNameQuery.AddHandlers(services, lifetime);
		global::MainCore.Commands.Queries.GetVillagesQuery.AddHandlers(services, lifetime);
		global::MainCore.Commands.Queries.VerifyAccessQuery.AddHandlers(services, lifetime);
		global::MainCore.Commands.UI.AccountSettingViewModel.GetSettingQuery.AddHandlers(services, lifetime);
		global::MainCore.Commands.UI.AddAccountsViewModel.AddAccountsCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.UI.AddAccountViewModel.AddAccountCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.UI.DebugViewModel.GetEndpointAdressQuery.AddHandlers(services, lifetime);
		global::MainCore.Commands.UI.DebugViewModel.GetLogQuery.AddHandlers(services, lifetime);
		global::MainCore.Commands.UI.DebugViewModel.GetTaskItemsQuery.AddHandlers(services, lifetime);
		global::MainCore.Commands.UI.EditAccountViewModel.GetAcccountQuery.AddHandlers(services, lifetime);
		global::MainCore.Commands.UI.EditAccountViewModel.UpdateAccountCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.UI.FarmingViewModel.ActivationCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.UI.FarmingViewModel.GetFarmListItemsQuery.AddHandlers(services, lifetime);
		global::MainCore.Commands.UI.MainLayoutViewModel.DeleteCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.UI.MainLayoutViewModel.GetAccountItemsQuery.AddHandlers(services, lifetime);
		global::MainCore.Commands.UI.MainLayoutViewModel.LoginCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.UI.MainLayoutViewModel.LogoutCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.UI.Misc.SaveAccountSettingCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.UI.Misc.SaveVillageSettingCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.UI.Villages.BuildViewModel.DeleteJobByVillageIdCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.UI.Villages.BuildViewModel.ExportCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.UI.Villages.BuildViewModel.GetJobItemsQuery.AddHandlers(services, lifetime);
		global::MainCore.Commands.UI.Villages.BuildViewModel.GetLayoutBuildingItemsQuery.AddHandlers(services, lifetime);
		global::MainCore.Commands.UI.Villages.BuildViewModel.GetNormalBuildingsQuery.AddHandlers(services, lifetime);
		global::MainCore.Commands.UI.Villages.BuildViewModel.GetQueueBuildingItemsQuery.AddHandlers(services, lifetime);
		global::MainCore.Commands.UI.Villages.BuildViewModel.ImportCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.UI.Villages.BuildViewModel.MoveCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.UI.Villages.BuildViewModel.NormalBuildCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.UI.Villages.BuildViewModel.ResourceBuildCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.UI.Villages.BuildViewModel.SwapCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.UI.Villages.BuildViewModel.UpgradeCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.UI.Villages.VillageSettingViewModel.GetSettingQuery.AddHandlers(services, lifetime);
		global::MainCore.Commands.UI.VillageViewModel.GetVillageItemsQuery.AddHandlers(services, lifetime);
		global::MainCore.Commands.Update.UpdateAccountInfoCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.Update.UpdateBuildingCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.Update.UpdateFarmlistCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.Update.UpdateInventoryCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.Update.UpdateStorageCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.Update.UpdateVillageListCommand.AddHandlers(services, lifetime);
		global::MainCore.Notification.Handlers.MainWindowLoad.ChromeDriverInstall.AddHandlers(services, lifetime);
		global::MainCore.Notification.Handlers.MainWindowLoad.ChromeExtensionInstall.AddHandlers(services, lifetime);
		global::MainCore.Notification.Handlers.MainWindowLoad.ChromeUserAgentInstall.AddHandlers(services, lifetime);
		global::MainCore.Notification.Handlers.MainWindowLoad.DatabaseInstall.AddHandlers(services, lifetime);
		global::MainCore.Notification.Handlers.MainWindowUnload.ChromeClean.AddHandlers(services, lifetime);
		global::MainCore.Notification.Handlers.MainWindowUnload.ProxyCacheClean.AddHandlers(services, lifetime);
		global::MainCore.Notification.Handlers.MainWindowUnload.UseragentClean.AddHandlers(services, lifetime);
		global::MainCore.Notification.Handlers.Refresh.AccountListRefresh.AddHandlers(services, lifetime);
		global::MainCore.Notification.Handlers.Refresh.AccountSettingRefresh.AddHandlers(services, lifetime);
		global::MainCore.Notification.Handlers.Refresh.BuildingListRefresh.AddHandlers(services, lifetime);
		global::MainCore.Notification.Handlers.Refresh.EndpointAddressRefresh.AddHandlers(services, lifetime);
		global::MainCore.Notification.Handlers.Refresh.FarmListRefresh.AddHandlers(services, lifetime);
		global::MainCore.Notification.Handlers.Refresh.JobListRefresh.AddHandlers(services, lifetime);
		global::MainCore.Notification.Handlers.Refresh.QueueRefresh.AddHandlers(services, lifetime);
		global::MainCore.Notification.Handlers.Refresh.StatusRefresh.AddHandlers(services, lifetime);
		global::MainCore.Notification.Handlers.Refresh.TaskListRefresh.AddHandlers(services, lifetime);
		global::MainCore.Notification.Handlers.Refresh.VillageListRefresh.AddHandlers(services, lifetime);
		global::MainCore.Notification.Handlers.Refresh.VillageSettingRefresh.AddHandlers(services, lifetime);
		global::MainCore.Notification.Handlers.Trigger.BuildingUpdateTaskTrigger.AddHandlers(services, lifetime);
		global::MainCore.Notification.Handlers.Trigger.ChangeWallTrigger.AddHandlers(services, lifetime);
		global::MainCore.Notification.Handlers.Trigger.ClaimQuestTaskTrigger.AddHandlers(services, lifetime);
		global::MainCore.Notification.Handlers.Trigger.CompleteImmediatelyTaskTrigger.AddHandlers(services, lifetime);
		global::MainCore.Notification.Handlers.Trigger.LoginTaskTrigger.AddHandlers(services, lifetime);
		global::MainCore.Notification.Handlers.Trigger.NpcTaskTrigger.AddHandlers(services, lifetime);
		global::MainCore.Notification.Handlers.Trigger.RefreshVillageTaskTrigger.AddHandlers(services, lifetime);
		global::MainCore.Notification.Handlers.Trigger.SleepTaskTrigger.AddHandlers(services, lifetime);
		global::MainCore.Notification.Handlers.Trigger.StartAdventureTaskTrigger.AddHandlers(services, lifetime);
		global::MainCore.Notification.Handlers.Trigger.TrainTroopTaskTrigger.AddHandlers(services, lifetime);
		global::MainCore.Notification.Handlers.Trigger.UpgradeBuildingTaskTrigger.AddHandlers(services, lifetime);
		global::MainCore.Notification.Message.AccountInfoUpdated.AddHandlers(services, lifetime);
		global::MainCore.Notification.Message.AccountInit.AddHandlers(services, lifetime);
		global::MainCore.Notification.Message.AccountLogout.AddHandlers(services, lifetime);
		global::MainCore.Notification.Message.AccountSettingUpdated.AddHandlers(services, lifetime);
		global::MainCore.Notification.Message.AccountUpdated.AddHandlers(services, lifetime);
		global::MainCore.Notification.Message.AdventureUpdated.AddHandlers(services, lifetime);
		global::MainCore.Notification.Message.CompleteImmediatelyMessage.AddHandlers(services, lifetime);
		global::MainCore.Notification.Message.FarmListUpdated.AddHandlers(services, lifetime);
		global::MainCore.Notification.Message.HeroItemUpdated.AddHandlers(services, lifetime);
		global::MainCore.Notification.Message.JobUpdated.AddHandlers(services, lifetime);
		global::MainCore.Notification.Message.MainWindowLoaded.AddHandlers(services, lifetime);
		global::MainCore.Notification.Message.MainWindowUnloaded.AddHandlers(services, lifetime);
		global::MainCore.Notification.Message.QuestUpdated.AddHandlers(services, lifetime);
		global::MainCore.Notification.Message.QueueBuildingUpdated.AddHandlers(services, lifetime);
		global::MainCore.Notification.Message.StatusUpdated.AddHandlers(services, lifetime);
		global::MainCore.Notification.Message.StorageUpdated.AddHandlers(services, lifetime);
		global::MainCore.Notification.Message.TaskUpdated.AddHandlers(services, lifetime);
		global::MainCore.Notification.Message.VillageSettingUpdated.AddHandlers(services, lifetime);
		global::MainCore.Notification.Message.VillageUpdated.AddHandlers(services, lifetime);
		
		return services;
	}
}
