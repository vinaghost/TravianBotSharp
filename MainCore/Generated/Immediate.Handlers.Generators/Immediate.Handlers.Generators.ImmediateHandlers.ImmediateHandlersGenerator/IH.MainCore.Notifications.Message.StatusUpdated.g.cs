using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Notifications.Message;

partial class StatusUpdated
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notifications.Message.StatusUpdated.Notification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notifications.Message.StatusUpdated.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Notifications.Message.StatusUpdated.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(StatusUpdated);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notifications.Message.StatusUpdated.Notification request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Notifications.Message.StatusUpdated.Notification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notifications.Handlers.Refresh.StatusRefresh.Handler _statusRefresh;
		private readonly global::MainCore.Notifications.Handlers.Refresh.EndpointAddressRefresh.Handler _endpointAddressRefresh;

		public HandleBehavior(
			global::MainCore.Notifications.Handlers.Refresh.StatusRefresh.Handler statusRefresh,
			global::MainCore.Notifications.Handlers.Refresh.EndpointAddressRefresh.Handler endpointAddressRefresh
		)
		{
			_statusRefresh = statusRefresh;
			_endpointAddressRefresh = endpointAddressRefresh;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notifications.Message.StatusUpdated.Notification request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Notifications.Message.StatusUpdated
				.HandleAsync(
					request
					, _statusRefresh
					, _endpointAddressRefresh
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
		services.Add(new(typeof(global::MainCore.Notifications.Message.StatusUpdated.Handler), typeof(global::MainCore.Notifications.Message.StatusUpdated.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notifications.Message.StatusUpdated.Notification, global::System.ValueTuple>), typeof(global::MainCore.Notifications.Message.StatusUpdated.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Notifications.Message.StatusUpdated.HandleBehavior), typeof(global::MainCore.Notifications.Message.StatusUpdated.HandleBehavior), lifetime));
		return services;
	}
}
