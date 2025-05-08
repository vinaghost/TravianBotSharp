using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.NextExecute;

partial class NextExecuteTrainTroopTaskCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Tasks.TrainTroopTask.Task, global::System.ValueTuple>
	{
		private readonly global::MainCore.Commands.NextExecute.NextExecuteTrainTroopTaskCommand.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Commands.NextExecute.NextExecuteTrainTroopTaskCommand.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(NextExecuteTrainTroopTaskCommand);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Tasks.TrainTroopTask.Task request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Tasks.TrainTroopTask.Task, global::System.ValueTuple>
	{
		private readonly global::MainCore.Services.ISettingService _settingService;

		public HandleBehavior(
			global::MainCore.Services.ISettingService settingService
		)
		{
			_settingService = settingService;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Tasks.TrainTroopTask.Task request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Commands.NextExecute.NextExecuteTrainTroopTaskCommand
				.HandleAsync(
					request
					, _settingService
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
		services.Add(new(typeof(global::MainCore.Commands.NextExecute.NextExecuteTrainTroopTaskCommand.Handler), typeof(global::MainCore.Commands.NextExecute.NextExecuteTrainTroopTaskCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Tasks.TrainTroopTask.Task, global::System.ValueTuple>), typeof(global::MainCore.Commands.NextExecute.NextExecuteTrainTroopTaskCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.NextExecute.NextExecuteTrainTroopTaskCommand.HandleBehavior), typeof(global::MainCore.Commands.NextExecute.NextExecuteTrainTroopTaskCommand.HandleBehavior), lifetime));
		return services;
	}
}
