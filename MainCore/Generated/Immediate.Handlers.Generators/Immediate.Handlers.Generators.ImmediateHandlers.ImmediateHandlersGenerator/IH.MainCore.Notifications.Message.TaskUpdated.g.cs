using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Notifications.Message;

partial class TaskUpdated
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notifications.Message.TaskUpdated.Notification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notifications.Message.TaskUpdated.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Notifications.Message.TaskUpdated.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(TaskUpdated);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notifications.Message.TaskUpdated.Notification request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Notifications.Message.TaskUpdated.Notification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notifications.Handlers.Refresh.TaskListRefresh.Handler _taskListRefresh;

		public HandleBehavior(
			global::MainCore.Notifications.Handlers.Refresh.TaskListRefresh.Handler taskListRefresh
		)
		{
			_taskListRefresh = taskListRefresh;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notifications.Message.TaskUpdated.Notification request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Notifications.Message.TaskUpdated
				.HandleAsync(
					request
					, _taskListRefresh
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
		services.Add(new(typeof(global::MainCore.Notifications.Message.TaskUpdated.Handler), typeof(global::MainCore.Notifications.Message.TaskUpdated.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notifications.Message.TaskUpdated.Notification, global::System.ValueTuple>), typeof(global::MainCore.Notifications.Message.TaskUpdated.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Notifications.Message.TaskUpdated.HandleBehavior), typeof(global::MainCore.Notifications.Message.TaskUpdated.HandleBehavior), lifetime));
		return services;
	}
}
