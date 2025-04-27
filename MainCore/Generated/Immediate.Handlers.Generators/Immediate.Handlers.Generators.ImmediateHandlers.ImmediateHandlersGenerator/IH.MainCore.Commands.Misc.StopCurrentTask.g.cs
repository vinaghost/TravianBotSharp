using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.Misc;

partial class StopCurrentTask
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Misc.StopCurrentTask.Command, global::System.ValueTuple>
	{
		private readonly global::MainCore.Commands.Misc.StopCurrentTask.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Commands.Misc.StopCurrentTask.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(StopCurrentTask);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.Misc.StopCurrentTask.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.Misc.StopCurrentTask.Command, global::System.ValueTuple>
	{
		private readonly global::MainCore.Services.ITaskManager _taskManager;

		public HandleBehavior(
			global::MainCore.Services.ITaskManager taskManager
		)
		{
			_taskManager = taskManager;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.Misc.StopCurrentTask.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Commands.Misc.StopCurrentTask
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
		services.Add(new(typeof(global::MainCore.Commands.Misc.StopCurrentTask.Handler), typeof(global::MainCore.Commands.Misc.StopCurrentTask.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Misc.StopCurrentTask.Command, global::System.ValueTuple>), typeof(global::MainCore.Commands.Misc.StopCurrentTask.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.Misc.StopCurrentTask.HandleBehavior), typeof(global::MainCore.Commands.Misc.StopCurrentTask.HandleBehavior), lifetime));
		return services;
	}
}
