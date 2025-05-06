using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Notification.Handlers.Trigger;

partial class SleepTaskTrigger
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Constraints.IAccountNotification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Handlers.Trigger.SleepTaskTrigger.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Notification.Handlers.Trigger.SleepTaskTrigger.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(SleepTaskTrigger);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Constraints.IAccountNotification request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Constraints.IAccountNotification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Services.ITaskManager _taskManager;
		private readonly global::MainCore.Infrasturecture.Persistence.AppDbContext _context;

		public HandleBehavior(
			global::MainCore.Services.ITaskManager taskManager,
			global::MainCore.Infrasturecture.Persistence.AppDbContext context
		)
		{
			_taskManager = taskManager;
			_context = context;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Constraints.IAccountNotification request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Notification.Handlers.Trigger.SleepTaskTrigger
				.HandleAsync(
					request
					, _taskManager
					, _context
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
		services.Add(new(typeof(global::MainCore.Notification.Handlers.Trigger.SleepTaskTrigger.Handler), typeof(global::MainCore.Notification.Handlers.Trigger.SleepTaskTrigger.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Constraints.IAccountNotification, global::System.ValueTuple>), typeof(global::MainCore.Notification.Handlers.Trigger.SleepTaskTrigger.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Notification.Handlers.Trigger.SleepTaskTrigger.HandleBehavior), typeof(global::MainCore.Notification.Handlers.Trigger.SleepTaskTrigger.HandleBehavior), lifetime));
		return services;
	}
}
