using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Notification.Handlers.Trigger;

partial class LoginTaskTrigger
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.Base.IAccountNotification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Handlers.Trigger.LoginTaskTrigger.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Notification.Handlers.Trigger.LoginTaskTrigger.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(LoginTaskTrigger);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.Base.IAccountNotification request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Notification.Base.IAccountNotification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Services.ITaskManager _taskManager;

		public HandleBehavior(
			global::MainCore.Services.ITaskManager taskManager
		)
		{
			_taskManager = taskManager;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.Base.IAccountNotification request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Notification.Handlers.Trigger.LoginTaskTrigger
				.HandleAsync(
					request
					, _taskManager
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
		services.Add(new(typeof(global::MainCore.Notification.Handlers.Trigger.LoginTaskTrigger.Handler), typeof(global::MainCore.Notification.Handlers.Trigger.LoginTaskTrigger.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.Base.IAccountNotification, global::System.ValueTuple>), typeof(global::MainCore.Notification.Handlers.Trigger.LoginTaskTrigger.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Notification.Handlers.Trigger.LoginTaskTrigger.HandleBehavior), typeof(global::MainCore.Notification.Handlers.Trigger.LoginTaskTrigger.HandleBehavior), lifetime));
		return services;
	}
}
