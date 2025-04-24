using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.Misc;

partial class StopCurrentTaskCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Misc.StopCurrentTaskCommand.Command, global::System.ValueTuple>
	{
		private readonly global::MainCore.Commands.Misc.StopCurrentTaskCommand.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Commands.Misc.StopCurrentTaskCommand.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(StopCurrentTaskCommand);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.Misc.StopCurrentTaskCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.Misc.StopCurrentTaskCommand.Command, global::System.ValueTuple>
	{
		private readonly global::MainCore.Services.ITaskManager _taskManager;
		private readonly global::MainCore.Services.ITimerManager _timerManager;

		public HandleBehavior(
			global::MainCore.Services.ITaskManager taskManager,
			global::MainCore.Services.ITimerManager timerManager
		)
		{
			_taskManager = taskManager;
			_timerManager = timerManager;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.Misc.StopCurrentTaskCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Commands.Misc.StopCurrentTaskCommand
				.HandleAsync(
					request
					, _taskManager
					, _timerManager
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
		services.Add(new(typeof(global::MainCore.Commands.Misc.StopCurrentTaskCommand.Handler), typeof(global::MainCore.Commands.Misc.StopCurrentTaskCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Misc.StopCurrentTaskCommand.Command, global::System.ValueTuple>), typeof(global::MainCore.Commands.Misc.StopCurrentTaskCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.Misc.StopCurrentTaskCommand.HandleBehavior), typeof(global::MainCore.Commands.Misc.StopCurrentTaskCommand.HandleBehavior), lifetime));
		return services;
	}
}
