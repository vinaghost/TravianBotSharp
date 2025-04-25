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
		global::MainCore.Commands.Misc.AddJobCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.Misc.DeleteJobByIdCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.Misc.StopCurrentTask.AddHandlers(services, lifetime);
		global::MainCore.Commands.Queries.GetBuilding.AddHandlers(services, lifetime);
		global::MainCore.Commands.Queries.GetLayoutBuildingsQuery.AddHandlers(services, lifetime);
		global::MainCore.Commands.Queries.IsResourceEnough.AddHandlers(services, lifetime);
		global::MainCore.Commands.UI.DebugViewModel.GetEndpointAdressQuery.AddHandlers(services, lifetime);
		global::MainCore.Commands.UI.DebugViewModel.GetLogQuery.AddHandlers(services, lifetime);
		global::MainCore.Commands.UI.DebugViewModel.GetTaskItemsQuery.AddHandlers(services, lifetime);
		global::MainCore.Commands.UI.FarmingViewModel.ActivationCommand.AddHandlers(services, lifetime);
		global::MainCore.Commands.UI.FarmingViewModel.GetFarmListItemsQuery.AddHandlers(services, lifetime);
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
