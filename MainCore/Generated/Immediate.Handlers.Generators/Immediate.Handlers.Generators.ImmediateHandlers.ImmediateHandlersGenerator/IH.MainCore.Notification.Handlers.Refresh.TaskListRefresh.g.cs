using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Notification.Handlers.Refresh;

partial class TaskListRefresh
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.ByAccountIdBase, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Handlers.Refresh.TaskListRefresh.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Notification.ByAccountIdBase, global::System.ValueTuple> _loggingBehavior;

		public Handler(
			global::MainCore.Notification.Handlers.Refresh.TaskListRefresh.HandleBehavior handleBehavior,
			global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Notification.ByAccountIdBase, global::System.ValueTuple> loggingBehavior
		)
		{
			var handlerType = typeof(TaskListRefresh);

			_handleBehavior = handleBehavior;

			_loggingBehavior = loggingBehavior;
			_loggingBehavior.HandlerType = handlerType;

			_loggingBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.ByAccountIdBase request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _loggingBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Notification.ByAccountIdBase, global::System.ValueTuple>
	{
		private readonly global::MainCore.UI.ViewModels.Tabs.DebugViewModel _viewModel;

		public HandleBehavior(
			global::MainCore.UI.ViewModels.Tabs.DebugViewModel viewModel
		)
		{
			_viewModel = viewModel;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.ByAccountIdBase request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Notification.Handlers.Refresh.TaskListRefresh
				.HandleAsync(
					request
					, _viewModel
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
		services.Add(new(typeof(global::MainCore.Notification.Handlers.Refresh.TaskListRefresh.Handler), typeof(global::MainCore.Notification.Handlers.Refresh.TaskListRefresh.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.ByAccountIdBase, global::System.ValueTuple>), typeof(global::MainCore.Notification.Handlers.Refresh.TaskListRefresh.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Notification.Handlers.Refresh.TaskListRefresh.HandleBehavior), typeof(global::MainCore.Notification.Handlers.Refresh.TaskListRefresh.HandleBehavior), lifetime));
		return services;
	}
}
