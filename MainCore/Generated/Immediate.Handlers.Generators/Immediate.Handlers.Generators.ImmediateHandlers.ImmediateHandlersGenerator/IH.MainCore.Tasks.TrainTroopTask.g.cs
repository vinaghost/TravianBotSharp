using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Tasks;

partial class TrainTroopTask
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Tasks.TrainTroopTask.Task, global::FluentResults.Result>
	{
		private readonly global::MainCore.Tasks.TrainTroopTask.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Tasks.Behaviors.VillageTaskBehavior<global::MainCore.Tasks.TrainTroopTask.Task, global::FluentResults.Result> _villageTaskBehavior;
		private readonly global::MainCore.Tasks.Behaviors.AccountTaskBehavior<global::MainCore.Tasks.TrainTroopTask.Task, global::FluentResults.Result> _accountTaskBehavior;

		public Handler(
			global::MainCore.Tasks.TrainTroopTask.HandleBehavior handleBehavior,
			global::MainCore.Tasks.Behaviors.VillageTaskBehavior<global::MainCore.Tasks.TrainTroopTask.Task, global::FluentResults.Result> villageTaskBehavior,
			global::MainCore.Tasks.Behaviors.AccountTaskBehavior<global::MainCore.Tasks.TrainTroopTask.Task, global::FluentResults.Result> accountTaskBehavior
		)
		{
			var handlerType = typeof(TrainTroopTask);

			_handleBehavior = handleBehavior;

			_accountTaskBehavior = accountTaskBehavior;
			_accountTaskBehavior.HandlerType = handlerType;

			_villageTaskBehavior = villageTaskBehavior;
			_villageTaskBehavior.HandlerType = handlerType;

			_villageTaskBehavior.SetInnerHandler(_handleBehavior);
			_accountTaskBehavior.SetInnerHandler(_villageTaskBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Tasks.TrainTroopTask.Task request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _accountTaskBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Tasks.TrainTroopTask.Task, global::FluentResults.Result>
	{
		private readonly global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> _contextFactory;
		private readonly global::MainCore.Services.ITaskManager _taskManager;
		private readonly global::MainCore.Commands.Features.TrainTroop.TrainTroopCommand.Handler _trainTroopCommand;
		private readonly global::MainCore.Commands.UI.Misc.SaveVillageSettingCommand.Handler _saveVillageSettingCommand;

		public HandleBehavior(
			global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> contextFactory,
			global::MainCore.Services.ITaskManager taskManager,
			global::MainCore.Commands.Features.TrainTroop.TrainTroopCommand.Handler trainTroopCommand,
			global::MainCore.Commands.UI.Misc.SaveVillageSettingCommand.Handler saveVillageSettingCommand
		)
		{
			_contextFactory = contextFactory;
			_taskManager = taskManager;
			_trainTroopCommand = trainTroopCommand;
			_saveVillageSettingCommand = saveVillageSettingCommand;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Tasks.TrainTroopTask.Task request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Tasks.TrainTroopTask
				.HandleAsync(
					request
					, _contextFactory
					, _taskManager
					, _trainTroopCommand
					, _saveVillageSettingCommand
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
		services.Add(new(typeof(global::MainCore.Tasks.TrainTroopTask.Handler), typeof(global::MainCore.Tasks.TrainTroopTask.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Tasks.TrainTroopTask.Task, global::FluentResults.Result>), typeof(global::MainCore.Tasks.TrainTroopTask.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Tasks.TrainTroopTask.HandleBehavior), typeof(global::MainCore.Tasks.TrainTroopTask.HandleBehavior), lifetime));
		return services;
	}
}
