using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

#pragma warning disable CS1591

namespace MainCore;

public static class HandlerServiceCollectionExtensions
{
	public static IServiceCollection AddMainCoreBehaviors(
		this IServiceCollection services)
	{
		services.TryAddTransient(typeof(global::MainCore.Commands.Behaviors.CommandLoggingBehavior<,>));
		services.TryAddTransient(typeof(global::MainCore.Tasks.Behaviors.AccountTaskBehavior<,>));
		services.TryAddTransient(typeof(global::MainCore.Tasks.Behaviors.VillageTaskBehavior<,>));
		services.TryAddTransient(typeof(global::MainCore.Notifications.Behaviors.CompleteImmediatelyBehavior<,>));
		services.TryAddTransient(typeof(global::MainCore.Notifications.Behaviors.AccountListUpdatedBehavior<,>));
		services.TryAddTransient(typeof(global::MainCore.Notifications.Behaviors.FarmListUpdatedBehavior<,>));
		services.TryAddTransient(typeof(global::MainCore.Notifications.Behaviors.AccountSettingUpdatedBehavior<,>));
		services.TryAddTransient(typeof(global::MainCore.Notifications.Behaviors.VillageSettingUpdatedBehavior<,>));
		services.TryAddTransient(typeof(global::MainCore.Notifications.Behaviors.AccountInfoUpdatedBehavior<,>));
		services.TryAddTransient(typeof(global::MainCore.Notifications.Behaviors.BuildingUpdatedBehavior<,>));
		services.TryAddTransient(typeof(global::MainCore.Notifications.Behaviors.StorageUpdatedBehavior<,>));
		services.TryAddTransient(typeof(global::MainCore.Notifications.Behaviors.VillageListUpdatedBehavior<,>));
		
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
		global::MainCore.Commands.NextExecute.NextExecuteSleepTaskCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.NextExecute.NextExecuteStartFarmListTaskCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.NextExecute.NextExecuteTrainTroopTaskCommand.AddHandlers(services, lifetime);
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
		global::MainCore.Commands.UI.Villages.BuildViewModel.FixJobsCommand.AddHandlers(services, lifetime);
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
		global::MainCore.Notifications.Handlers.MainWindowLoad.ChromeDriverInstall.AddHandlers(services, lifetime);
		global::MainCore.Notifications.Handlers.MainWindowLoad.ChromeExtensionInstall.AddHandlers(services, lifetime);
		global::MainCore.Notifications.Handlers.MainWindowLoad.ChromeUserAgentInstall.AddHandlers(services, lifetime);
		global::MainCore.Notifications.Handlers.MainWindowLoad.DatabaseInstall.AddHandlers(services, lifetime);
		global::MainCore.Notifications.Handlers.MainWindowUnload.ChromeClean.AddHandlers(services, lifetime);
		global::MainCore.Notifications.Handlers.MainWindowUnload.ProxyCacheClean.AddHandlers(services, lifetime);
		global::MainCore.Notifications.Handlers.MainWindowUnload.UseragentClean.AddHandlers(services, lifetime);
		global::MainCore.Notifications.Handlers.Refresh.AccountListRefresh.AddHandlers(services, lifetime);
		global::MainCore.Notifications.Handlers.Refresh.AccountSettingRefresh.AddHandlers(services, lifetime);
		global::MainCore.Notifications.Handlers.Refresh.BuildingListRefresh.AddHandlers(services, lifetime);
		global::MainCore.Notifications.Handlers.Refresh.EndpointAddressRefresh.AddHandlers(services, lifetime);
		global::MainCore.Notifications.Handlers.Refresh.FarmListRefresh.AddHandlers(services, lifetime);
		global::MainCore.Notifications.Handlers.Refresh.JobListRefresh.AddHandlers(services, lifetime);
		global::MainCore.Notifications.Handlers.Refresh.QueueRefresh.AddHandlers(services, lifetime);
		global::MainCore.Notifications.Handlers.Refresh.StatusRefresh.AddHandlers(services, lifetime);
		global::MainCore.Notifications.Handlers.Refresh.TaskListRefresh.AddHandlers(services, lifetime);
		global::MainCore.Notifications.Handlers.Refresh.VillageListRefresh.AddHandlers(services, lifetime);
		global::MainCore.Notifications.Handlers.Refresh.VillageSettingRefresh.AddHandlers(services, lifetime);
		global::MainCore.Notifications.Handlers.Trigger.BuildingUpdateTaskTrigger.AddHandlers(services, lifetime);
		global::MainCore.Notifications.Handlers.Trigger.ChangeWallTrigger.AddHandlers(services, lifetime);
		global::MainCore.Notifications.Handlers.Trigger.ClaimQuestTaskTrigger.AddHandlers(services, lifetime);
		global::MainCore.Notifications.Handlers.Trigger.CompleteImmediatelyTaskTrigger.AddHandlers(services, lifetime);
		global::MainCore.Notifications.Handlers.Trigger.LoginTaskTrigger.AddHandlers(services, lifetime);
		global::MainCore.Notifications.Handlers.Trigger.NpcTaskTrigger.AddHandlers(services, lifetime);
		global::MainCore.Notifications.Handlers.Trigger.RefreshVillageTaskTrigger.AddHandlers(services, lifetime);
		global::MainCore.Notifications.Handlers.Trigger.SleepTaskTrigger.AddHandlers(services, lifetime);
		global::MainCore.Notifications.Handlers.Trigger.StartAdventureTaskTrigger.AddHandlers(services, lifetime);
		global::MainCore.Notifications.Handlers.Trigger.TrainTroopTaskTrigger.AddHandlers(services, lifetime);
		global::MainCore.Notifications.Handlers.Trigger.UpgradeBuildingTaskTrigger.AddHandlers(services, lifetime);
		global::MainCore.Notifications.Message.AccountInit.AddHandlers(services, lifetime);
		global::MainCore.Notifications.Message.JobUpdated.AddHandlers(services, lifetime);
		global::MainCore.Notifications.Message.MainWindowLoaded.AddHandlers(services, lifetime);
		global::MainCore.Notifications.Message.MainWindowUnloaded.AddHandlers(services, lifetime);
		global::MainCore.Notifications.Message.StatusUpdated.AddHandlers(services, lifetime);
		global::MainCore.Notifications.Message.TaskUpdated.AddHandlers(services, lifetime);
		global::MainCore.Queries.GetAccessesQuery.AddHandlers(services, lifetime);
		global::MainCore.Queries.GetActiveFarmsQuery.AddHandlers(services, lifetime);
		global::MainCore.Queries.GetBuildingLocationQuery.AddHandlers(services, lifetime);
		global::MainCore.Queries.GetBuildingQuery.AddHandlers(services, lifetime);
		global::MainCore.Queries.GetFirstQueueBuildingQuery.AddHandlers(services, lifetime);
		global::MainCore.Queries.GetHasBuildJobVillagesQuery.AddHandlers(services, lifetime);
		global::MainCore.Queries.GetHeroItemsQuery.AddHandlers(services, lifetime);
		global::MainCore.Queries.GetJobQuery.AddHandlers(services, lifetime);
		global::MainCore.Queries.GetLayoutBuildingsQuery.AddHandlers(services, lifetime);
		global::MainCore.Queries.GetLowestBuildingQuery.AddHandlers(services, lifetime);
		global::MainCore.Queries.GetMissingBuildingVillagesQuery.AddHandlers(services, lifetime);
		global::MainCore.Queries.GetTrainTroopBuildingQuery.AddHandlers(services, lifetime);
		global::MainCore.Queries.GetValidAccessQuery.AddHandlers(services, lifetime);
		global::MainCore.Queries.GetVillageHasRallypointQuery.AddHandlers(services, lifetime);
		global::MainCore.Queries.GetVillageNameQuery.AddHandlers(services, lifetime);
		global::MainCore.Queries.GetVillagesQuery.AddHandlers(services, lifetime);
		global::MainCore.Queries.VerifyAccessQuery.AddHandlers(services, lifetime);
		global::MainCore.Tasks.ClaimQuestTask.AddHandlers(services, lifetime);
		global::MainCore.Tasks.CompleteImmediatelyTask.AddHandlers(services, lifetime);
		global::MainCore.Tasks.LoginTask.AddHandlers(services, lifetime);
		global::MainCore.Tasks.NpcTask.AddHandlers(services, lifetime);
		global::MainCore.Tasks.SleepTask.AddHandlers(services, lifetime);
		global::MainCore.Tasks.StartAdventureTask.AddHandlers(services, lifetime);
		global::MainCore.Tasks.StartFarmListTask.AddHandlers(services, lifetime);
		global::MainCore.Tasks.TrainTroopTask.AddHandlers(services, lifetime);
		global::MainCore.Tasks.UpdateBuildingTask.AddHandlers(services, lifetime);
		global::MainCore.Tasks.UpdateFarmListTask.AddHandlers(services, lifetime);
		global::MainCore.Tasks.UpdateVillageTask.AddHandlers(services, lifetime);
		global::MainCore.Tasks.UpgradeBuildingTask.AddHandlers(services, lifetime);
		
		return services;
	}
}
