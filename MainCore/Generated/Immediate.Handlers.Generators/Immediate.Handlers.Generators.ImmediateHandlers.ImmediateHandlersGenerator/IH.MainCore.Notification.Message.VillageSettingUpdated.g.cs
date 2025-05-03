using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Notification.Message;

partial class VillageSettingUpdated
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.Message.VillageSettingUpdated.Notification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Message.VillageSettingUpdated.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Notification.Message.VillageSettingUpdated.Notification, global::System.ValueTuple> _loggingBehavior;

		public Handler(
			global::MainCore.Notification.Message.VillageSettingUpdated.HandleBehavior handleBehavior,
			global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Notification.Message.VillageSettingUpdated.Notification, global::System.ValueTuple> loggingBehavior
		)
		{
			var handlerType = typeof(VillageSettingUpdated);

			_handleBehavior = handleBehavior;

			_loggingBehavior = loggingBehavior;
			_loggingBehavior.HandlerType = handlerType;

			_loggingBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.Message.VillageSettingUpdated.Notification request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _loggingBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Notification.Message.VillageSettingUpdated.Notification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Handlers.Trigger.ChangeWallTrigger.Handler _changeWallTrigger;
		private readonly global::MainCore.Notification.Handlers.Trigger.ClaimQuestTaskTrigger.Handler _claimQuestTaskTrigger;
		private readonly global::MainCore.Notification.Handlers.Trigger.CompleteImmediatelyTaskTrigger.Handler _completeImmediatelyTaskTrigger;
		private readonly global::MainCore.Notification.Handlers.Trigger.NpcTaskTrigger.Handler _npcTaskTrigger;
		private readonly global::MainCore.Notification.Handlers.Trigger.RefreshVillageTaskTrigger.Handler _refreshVillageTaskTrigger;
		private readonly global::MainCore.Notification.Handlers.Trigger.TrainTroopTaskTrigger.Handler _trainTroopTaskTrigger;
		private readonly global::MainCore.Notification.Handlers.Refresh.VillageSettingRefresh.Handler _villageSettingRefresh;

		public HandleBehavior(
			global::MainCore.Notification.Handlers.Trigger.ChangeWallTrigger.Handler changeWallTrigger,
			global::MainCore.Notification.Handlers.Trigger.ClaimQuestTaskTrigger.Handler claimQuestTaskTrigger,
			global::MainCore.Notification.Handlers.Trigger.CompleteImmediatelyTaskTrigger.Handler completeImmediatelyTaskTrigger,
			global::MainCore.Notification.Handlers.Trigger.NpcTaskTrigger.Handler npcTaskTrigger,
			global::MainCore.Notification.Handlers.Trigger.RefreshVillageTaskTrigger.Handler refreshVillageTaskTrigger,
			global::MainCore.Notification.Handlers.Trigger.TrainTroopTaskTrigger.Handler trainTroopTaskTrigger,
			global::MainCore.Notification.Handlers.Refresh.VillageSettingRefresh.Handler villageSettingRefresh
		)
		{
			_changeWallTrigger = changeWallTrigger;
			_claimQuestTaskTrigger = claimQuestTaskTrigger;
			_completeImmediatelyTaskTrigger = completeImmediatelyTaskTrigger;
			_npcTaskTrigger = npcTaskTrigger;
			_refreshVillageTaskTrigger = refreshVillageTaskTrigger;
			_trainTroopTaskTrigger = trainTroopTaskTrigger;
			_villageSettingRefresh = villageSettingRefresh;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.Message.VillageSettingUpdated.Notification request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Notification.Message.VillageSettingUpdated
				.HandleAsync(
					request
					, _changeWallTrigger
					, _claimQuestTaskTrigger
					, _completeImmediatelyTaskTrigger
					, _npcTaskTrigger
					, _refreshVillageTaskTrigger
					, _trainTroopTaskTrigger
					, _villageSettingRefresh
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
		services.Add(new(typeof(global::MainCore.Notification.Message.VillageSettingUpdated.Handler), typeof(global::MainCore.Notification.Message.VillageSettingUpdated.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.Message.VillageSettingUpdated.Notification, global::System.ValueTuple>), typeof(global::MainCore.Notification.Message.VillageSettingUpdated.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Notification.Message.VillageSettingUpdated.HandleBehavior), typeof(global::MainCore.Notification.Message.VillageSettingUpdated.HandleBehavior), lifetime));
		return services;
	}
}
