using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Notification.Message;

partial class AccountInit
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.Message.AccountInit.Notification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Message.AccountInit.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Notification.Message.AccountInit.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(AccountInit);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.Message.AccountInit.Notification request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Notification.Message.AccountInit.Notification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Handlers.Trigger.LoginTaskTrigger.Handler _loginTaskTrigger;
		private readonly global::MainCore.Commands.Queries.GetVillage _getVillage;
		private readonly global::MainCore.Notification.Handlers.Trigger.RefreshVillageTaskTrigger.Handler _refreshVillageTaskTrigger;
		private readonly global::MainCore.Notification.Handlers.Trigger.SleepTaskTrigger.Handler _sleepTaskTrigger;
		private readonly global::MainCore.Notification.Handlers.Trigger.StartAdventureTaskTrigger.Handler _startAdventureTaskTrigger;
		private readonly global::MainCore.Notification.Handlers.Trigger.TrainTroopTaskTrigger.Handler _trainTroopTaskTrigger;
		private readonly global::MainCore.Notification.Handlers.Trigger.UpgradeBuildingTaskTrigger.Handler _upgradeBuildingTaskTrigger;

		public HandleBehavior(
			global::MainCore.Notification.Handlers.Trigger.LoginTaskTrigger.Handler loginTaskTrigger,
			global::MainCore.Commands.Queries.GetVillage getVillage,
			global::MainCore.Notification.Handlers.Trigger.RefreshVillageTaskTrigger.Handler refreshVillageTaskTrigger,
			global::MainCore.Notification.Handlers.Trigger.SleepTaskTrigger.Handler sleepTaskTrigger,
			global::MainCore.Notification.Handlers.Trigger.StartAdventureTaskTrigger.Handler startAdventureTaskTrigger,
			global::MainCore.Notification.Handlers.Trigger.TrainTroopTaskTrigger.Handler trainTroopTaskTrigger,
			global::MainCore.Notification.Handlers.Trigger.UpgradeBuildingTaskTrigger.Handler upgradeBuildingTaskTrigger
		)
		{
			_loginTaskTrigger = loginTaskTrigger;
			_getVillage = getVillage;
			_refreshVillageTaskTrigger = refreshVillageTaskTrigger;
			_sleepTaskTrigger = sleepTaskTrigger;
			_startAdventureTaskTrigger = startAdventureTaskTrigger;
			_trainTroopTaskTrigger = trainTroopTaskTrigger;
			_upgradeBuildingTaskTrigger = upgradeBuildingTaskTrigger;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.Message.AccountInit.Notification request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Notification.Message.AccountInit
				.HandleAsync(
					request
					, _loginTaskTrigger
					, _getVillage
					, _refreshVillageTaskTrigger
					, _sleepTaskTrigger
					, _startAdventureTaskTrigger
					, _trainTroopTaskTrigger
					, _upgradeBuildingTaskTrigger
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
		services.Add(new(typeof(global::MainCore.Notification.Message.AccountInit.Handler), typeof(global::MainCore.Notification.Message.AccountInit.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.Message.AccountInit.Notification, global::System.ValueTuple>), typeof(global::MainCore.Notification.Message.AccountInit.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Notification.Message.AccountInit.HandleBehavior), typeof(global::MainCore.Notification.Message.AccountInit.HandleBehavior), lifetime));
		return services;
	}
}
