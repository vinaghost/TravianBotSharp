using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Notification.Message;

partial class StatusUpdated
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.Message.StatusUpdated.Notification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Message.StatusUpdated.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Notification.Message.StatusUpdated.Notification, global::System.ValueTuple> _loggingBehavior;

		public Handler(
			global::MainCore.Notification.Message.StatusUpdated.HandleBehavior handleBehavior,
			global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Notification.Message.StatusUpdated.Notification, global::System.ValueTuple> loggingBehavior
		)
		{
			var handlerType = typeof(StatusUpdated);

			_handleBehavior = handleBehavior;

			_loggingBehavior = loggingBehavior;
			_loggingBehavior.HandlerType = handlerType;

			_loggingBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.Message.StatusUpdated.Notification request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _loggingBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Notification.Message.StatusUpdated.Notification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Handlers.Refresh.StatusRefresh.Handler _statusRefresh;
		private readonly global::MainCore.Notification.Handlers.Refresh.EndpointAddressRefresh.Handler _endpointAddressRefresh;

		public HandleBehavior(
			global::MainCore.Notification.Handlers.Refresh.StatusRefresh.Handler statusRefresh,
			global::MainCore.Notification.Handlers.Refresh.EndpointAddressRefresh.Handler endpointAddressRefresh
		)
		{
			_statusRefresh = statusRefresh;
			_endpointAddressRefresh = endpointAddressRefresh;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.Message.StatusUpdated.Notification request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Notification.Message.StatusUpdated
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
		services.Add(new(typeof(global::MainCore.Notification.Message.StatusUpdated.Handler), typeof(global::MainCore.Notification.Message.StatusUpdated.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.Message.StatusUpdated.Notification, global::System.ValueTuple>), typeof(global::MainCore.Notification.Message.StatusUpdated.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Notification.Message.StatusUpdated.HandleBehavior), typeof(global::MainCore.Notification.Message.StatusUpdated.HandleBehavior), lifetime));
		return services;
	}
}
