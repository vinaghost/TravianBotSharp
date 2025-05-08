using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Notification.Message;

partial class CompleteImmediatelyMessage
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.Message.CompleteImmediatelyMessage.Notification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Message.CompleteImmediatelyMessage.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Notification.Message.CompleteImmediatelyMessage.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(CompleteImmediatelyMessage);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.Message.CompleteImmediatelyMessage.Notification request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Notification.Message.CompleteImmediatelyMessage.Notification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Handlers.Trigger.UpgradeBuildingTaskTrigger.Handler _upgradeBuildingTaskTrigger;

		public HandleBehavior(
			global::MainCore.Notification.Handlers.Trigger.UpgradeBuildingTaskTrigger.Handler upgradeBuildingTaskTrigger
		)
		{
			_upgradeBuildingTaskTrigger = upgradeBuildingTaskTrigger;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.Message.CompleteImmediatelyMessage.Notification request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Notification.Message.CompleteImmediatelyMessage
				.HandleAsync(
					request
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
		services.Add(new(typeof(global::MainCore.Notification.Message.CompleteImmediatelyMessage.Handler), typeof(global::MainCore.Notification.Message.CompleteImmediatelyMessage.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.Message.CompleteImmediatelyMessage.Notification, global::System.ValueTuple>), typeof(global::MainCore.Notification.Message.CompleteImmediatelyMessage.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Notification.Message.CompleteImmediatelyMessage.HandleBehavior), typeof(global::MainCore.Notification.Message.CompleteImmediatelyMessage.HandleBehavior), lifetime));
		return services;
	}
}
