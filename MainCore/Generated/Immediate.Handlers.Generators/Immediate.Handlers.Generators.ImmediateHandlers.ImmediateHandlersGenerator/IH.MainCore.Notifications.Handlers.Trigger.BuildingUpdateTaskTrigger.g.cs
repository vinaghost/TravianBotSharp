using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Notifications.Handlers.Trigger;

partial class BuildingUpdateTaskTrigger
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Constraints.IAccountConstraint, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notifications.Handlers.Trigger.BuildingUpdateTaskTrigger.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Notifications.Handlers.Trigger.BuildingUpdateTaskTrigger.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(BuildingUpdateTaskTrigger);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Constraints.IAccountConstraint request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Constraints.IAccountConstraint, global::System.ValueTuple>
	{
		private readonly global::MainCore.Queries.GetVillageNameQuery.Handler _getVillageNameQuery;
		private readonly global::MainCore.Services.ITaskManager _taskManager;
		private readonly global::MainCore.Services.ISettingService _settingService;
		private readonly global::MainCore.Queries.GetMissingBuildingVillagesQuery.Handler _getMissingBuildingVillageQuery;

		public HandleBehavior(
			global::MainCore.Queries.GetVillageNameQuery.Handler getVillageNameQuery,
			global::MainCore.Services.ITaskManager taskManager,
			global::MainCore.Services.ISettingService settingService,
			global::MainCore.Queries.GetMissingBuildingVillagesQuery.Handler getMissingBuildingVillageQuery
		)
		{
			_getVillageNameQuery = getVillageNameQuery;
			_taskManager = taskManager;
			_settingService = settingService;
			_getMissingBuildingVillageQuery = getMissingBuildingVillageQuery;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Constraints.IAccountConstraint request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Notifications.Handlers.Trigger.BuildingUpdateTaskTrigger
				.HandleAsync(
					request
					, _getVillageNameQuery
					, _taskManager
					, _settingService
					, _getMissingBuildingVillageQuery
					, cancellationToken
				)
				.ConfigureAwait(false);

			return default;
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public static IServiceCollection AddHandlers(
		IServiceCollection services,
		ServiceLifetime lifetime = ServiceLifetime.Scoped
	)
	{
		services.Add(new(typeof(global::MainCore.Notifications.Handlers.Trigger.BuildingUpdateTaskTrigger.Handler), typeof(global::MainCore.Notifications.Handlers.Trigger.BuildingUpdateTaskTrigger.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Constraints.IAccountConstraint, global::System.ValueTuple>), typeof(global::MainCore.Notifications.Handlers.Trigger.BuildingUpdateTaskTrigger.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Notifications.Handlers.Trigger.BuildingUpdateTaskTrigger.HandleBehavior), typeof(global::MainCore.Notifications.Handlers.Trigger.BuildingUpdateTaskTrigger.HandleBehavior), lifetime));
		return services;
	}
}
