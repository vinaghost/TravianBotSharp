using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Notification.Message;

partial class BuildingUpdated
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.Message.BuildingUpdated.Notification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Message.BuildingUpdated.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Notification.Message.BuildingUpdated.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(BuildingUpdated);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.Message.BuildingUpdated.Notification request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Notification.Message.BuildingUpdated.Notification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Handlers.Refresh.BuildingListRefresh.Handler _buildingListRefresh;

		public HandleBehavior(
			global::MainCore.Notification.Handlers.Refresh.BuildingListRefresh.Handler buildingListRefresh
		)
		{
			_buildingListRefresh = buildingListRefresh;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.Message.BuildingUpdated.Notification request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Notification.Message.BuildingUpdated
				.HandleAsync(
					request
					, _buildingListRefresh
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
		services.Add(new(typeof(global::MainCore.Notification.Message.BuildingUpdated.Handler), typeof(global::MainCore.Notification.Message.BuildingUpdated.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.Message.BuildingUpdated.Notification, global::System.ValueTuple>), typeof(global::MainCore.Notification.Message.BuildingUpdated.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Notification.Message.BuildingUpdated.HandleBehavior), typeof(global::MainCore.Notification.Message.BuildingUpdated.HandleBehavior), lifetime));
		return services;
	}
}
