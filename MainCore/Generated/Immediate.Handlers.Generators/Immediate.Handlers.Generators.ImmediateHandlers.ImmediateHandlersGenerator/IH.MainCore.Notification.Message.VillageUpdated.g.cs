using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Notification.Message;

partial class VillageUpdated
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.Message.VillageUpdated.Notification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Message.VillageUpdated.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Notification.Message.VillageUpdated.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(VillageUpdated);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.Message.VillageUpdated.Notification request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Notification.Message.VillageUpdated.Notification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Handlers.Trigger.BuildingUpdateTaskTrigger.Handler _buildingUpdateTaskTrigger;
		private readonly global::MainCore.Notification.Handlers.Refresh.VillageListRefresh.Handler _villageListRefresh;

		public HandleBehavior(
			global::MainCore.Notification.Handlers.Trigger.BuildingUpdateTaskTrigger.Handler buildingUpdateTaskTrigger,
			global::MainCore.Notification.Handlers.Refresh.VillageListRefresh.Handler villageListRefresh
		)
		{
			_buildingUpdateTaskTrigger = buildingUpdateTaskTrigger;
			_villageListRefresh = villageListRefresh;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.Message.VillageUpdated.Notification request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Notification.Message.VillageUpdated
				.HandleAsync(
					request
					, _buildingUpdateTaskTrigger
					, _villageListRefresh
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
		services.Add(new(typeof(global::MainCore.Notification.Message.VillageUpdated.Handler), typeof(global::MainCore.Notification.Message.VillageUpdated.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.Message.VillageUpdated.Notification, global::System.ValueTuple>), typeof(global::MainCore.Notification.Message.VillageUpdated.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Notification.Message.VillageUpdated.HandleBehavior), typeof(global::MainCore.Notification.Message.VillageUpdated.HandleBehavior), lifetime));
		return services;
	}
}
