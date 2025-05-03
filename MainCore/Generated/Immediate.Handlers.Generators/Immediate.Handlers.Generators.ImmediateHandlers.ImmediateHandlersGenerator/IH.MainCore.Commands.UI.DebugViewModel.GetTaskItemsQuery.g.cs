using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.UI.DebugViewModel;

partial class GetTaskItemsQuery
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.DebugViewModel.GetTaskItemsQuery.Query, global::System.Collections.Generic.List<global::MainCore.UI.Models.Output.TaskItem>>
	{
		private readonly global::MainCore.Commands.UI.DebugViewModel.GetTaskItemsQuery.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Commands.UI.DebugViewModel.GetTaskItemsQuery.Query, global::System.Collections.Generic.List<global::MainCore.UI.Models.Output.TaskItem>> _loggingBehavior;

		public Handler(
			global::MainCore.Commands.UI.DebugViewModel.GetTaskItemsQuery.HandleBehavior handleBehavior,
			global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Commands.UI.DebugViewModel.GetTaskItemsQuery.Query, global::System.Collections.Generic.List<global::MainCore.UI.Models.Output.TaskItem>> loggingBehavior
		)
		{
			var handlerType = typeof(GetTaskItemsQuery);

			_handleBehavior = handleBehavior;

			_loggingBehavior = loggingBehavior;
			_loggingBehavior.HandlerType = handlerType;

			_loggingBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::System.Collections.Generic.List<global::MainCore.UI.Models.Output.TaskItem>> HandleAsync(
			global::MainCore.Commands.UI.DebugViewModel.GetTaskItemsQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _loggingBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.UI.DebugViewModel.GetTaskItemsQuery.Query, global::System.Collections.Generic.List<global::MainCore.UI.Models.Output.TaskItem>>
	{
		private readonly global::MainCore.Services.ITaskManager _taskManager;

		public HandleBehavior(
			global::MainCore.Services.ITaskManager taskManager
		)
		{
			_taskManager = taskManager;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.Collections.Generic.List<global::MainCore.UI.Models.Output.TaskItem>> HandleAsync(
			global::MainCore.Commands.UI.DebugViewModel.GetTaskItemsQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Commands.UI.DebugViewModel.GetTaskItemsQuery
				.HandleAsync(
					request
					, _taskManager
					, cancellationToken
				)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public static IServiceCollection AddHandlers(
		IServiceCollection services,
		ServiceLifetime lifetime = ServiceLifetime.Scoped
	)
	{
		services.Add(new(typeof(global::MainCore.Commands.UI.DebugViewModel.GetTaskItemsQuery.Handler), typeof(global::MainCore.Commands.UI.DebugViewModel.GetTaskItemsQuery.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.DebugViewModel.GetTaskItemsQuery.Query, global::System.Collections.Generic.List<global::MainCore.UI.Models.Output.TaskItem>>), typeof(global::MainCore.Commands.UI.DebugViewModel.GetTaskItemsQuery.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.UI.DebugViewModel.GetTaskItemsQuery.HandleBehavior), typeof(global::MainCore.Commands.UI.DebugViewModel.GetTaskItemsQuery.HandleBehavior), lifetime));
		return services;
	}
}
