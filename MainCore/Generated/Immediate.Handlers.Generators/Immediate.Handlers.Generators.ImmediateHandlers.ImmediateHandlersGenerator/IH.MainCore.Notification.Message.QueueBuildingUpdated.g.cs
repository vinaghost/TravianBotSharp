using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Notification.Message;

partial class QueueBuildingUpdated
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.Message.QueueBuildingUpdated.Notification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Message.QueueBuildingUpdated.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Notification.Message.QueueBuildingUpdated.Notification, global::System.ValueTuple> _loggingBehavior;

		public Handler(
			global::MainCore.Notification.Message.QueueBuildingUpdated.HandleBehavior handleBehavior,
			global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Notification.Message.QueueBuildingUpdated.Notification, global::System.ValueTuple> loggingBehavior
		)
		{
			var handlerType = typeof(QueueBuildingUpdated);

			_handleBehavior = handleBehavior;

			_loggingBehavior = loggingBehavior;
			_loggingBehavior.HandlerType = handlerType;

			_loggingBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.Message.QueueBuildingUpdated.Notification request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _loggingBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Notification.Message.QueueBuildingUpdated.Notification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Handlers.Trigger.CompleteImmediatelyTaskTrigger.Handler _completeImmediatelyTaskTrigger;
		private readonly global::MainCore.Notification.Handlers.Refresh.QueueRefresh.Handler _queueRefresh;

		public HandleBehavior(
			global::MainCore.Notification.Handlers.Trigger.CompleteImmediatelyTaskTrigger.Handler completeImmediatelyTaskTrigger,
			global::MainCore.Notification.Handlers.Refresh.QueueRefresh.Handler queueRefresh
		)
		{
			_completeImmediatelyTaskTrigger = completeImmediatelyTaskTrigger;
			_queueRefresh = queueRefresh;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.Message.QueueBuildingUpdated.Notification request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Notification.Message.QueueBuildingUpdated
				.HandleAsync(
					request
					, _completeImmediatelyTaskTrigger
					, _queueRefresh
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
		services.Add(new(typeof(global::MainCore.Notification.Message.QueueBuildingUpdated.Handler), typeof(global::MainCore.Notification.Message.QueueBuildingUpdated.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.Message.QueueBuildingUpdated.Notification, global::System.ValueTuple>), typeof(global::MainCore.Notification.Message.QueueBuildingUpdated.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Notification.Message.QueueBuildingUpdated.HandleBehavior), typeof(global::MainCore.Notification.Message.QueueBuildingUpdated.HandleBehavior), lifetime));
		return services;
	}
}
