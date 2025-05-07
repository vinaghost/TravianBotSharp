using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Notifications.Handlers.Trigger;

partial class ClaimQuestTaskTrigger
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Constraints.IVillageConstraint, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notifications.Handlers.Trigger.ClaimQuestTaskTrigger.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Notifications.Handlers.Trigger.ClaimQuestTaskTrigger.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(ClaimQuestTaskTrigger);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Constraints.IVillageConstraint request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Constraints.IVillageConstraint, global::System.ValueTuple>
	{
		private readonly global::MainCore.Queries.GetVillageNameQuery.Handler _getVillageNameQuery;
		private readonly global::MainCore.Services.ITaskManager _taskManager;
		private readonly global::MainCore.Services.ISettingService _settingService;

		public HandleBehavior(
			global::MainCore.Queries.GetVillageNameQuery.Handler getVillageNameQuery,
			global::MainCore.Services.ITaskManager taskManager,
			global::MainCore.Services.ISettingService settingService
		)
		{
			_getVillageNameQuery = getVillageNameQuery;
			_taskManager = taskManager;
			_settingService = settingService;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Constraints.IVillageConstraint request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Notifications.Handlers.Trigger.ClaimQuestTaskTrigger
				.HandleAsync(
					request
					, _getVillageNameQuery
					, _taskManager
					, _settingService
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
		services.Add(new(typeof(global::MainCore.Notifications.Handlers.Trigger.ClaimQuestTaskTrigger.Handler), typeof(global::MainCore.Notifications.Handlers.Trigger.ClaimQuestTaskTrigger.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Constraints.IVillageConstraint, global::System.ValueTuple>), typeof(global::MainCore.Notifications.Handlers.Trigger.ClaimQuestTaskTrigger.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Notifications.Handlers.Trigger.ClaimQuestTaskTrigger.HandleBehavior), typeof(global::MainCore.Notifications.Handlers.Trigger.ClaimQuestTaskTrigger.HandleBehavior), lifetime));
		return services;
	}
}
