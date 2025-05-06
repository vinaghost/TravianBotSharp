using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.NextExecute;

partial class NextExecuteSleepTaskCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Tasks.SleepTask.Task, global::System.ValueTuple>
	{
		private readonly global::MainCore.Commands.NextExecute.NextExecuteSleepTaskCommand.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Commands.NextExecute.NextExecuteSleepTaskCommand.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(NextExecuteSleepTaskCommand);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Tasks.SleepTask.Task request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Tasks.SleepTask.Task, global::System.ValueTuple>
	{
		private readonly global::MainCore.Services.ISettingService _settingService;
		private readonly global::MainCore.Services.ITaskManager _taskManager;

		public HandleBehavior(
			global::MainCore.Services.ISettingService settingService,
			global::MainCore.Services.ITaskManager taskManager
		)
		{
			_settingService = settingService;
			_taskManager = taskManager;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Tasks.SleepTask.Task request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Commands.NextExecute.NextExecuteSleepTaskCommand
				.HandleAsync(
					request
					, _settingService
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
		services.Add(new(typeof(global::MainCore.Commands.NextExecute.NextExecuteSleepTaskCommand.Handler), typeof(global::MainCore.Commands.NextExecute.NextExecuteSleepTaskCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Tasks.SleepTask.Task, global::System.ValueTuple>), typeof(global::MainCore.Commands.NextExecute.NextExecuteSleepTaskCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.NextExecute.NextExecuteSleepTaskCommand.HandleBehavior), typeof(global::MainCore.Commands.NextExecute.NextExecuteSleepTaskCommand.HandleBehavior), lifetime));
		return services;
	}
}
