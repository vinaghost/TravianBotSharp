using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Tasks;

partial class UpdateVillageTask
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Tasks.UpdateVillageTask.Task, global::FluentResults.Result>
	{
		private readonly global::MainCore.Tasks.UpdateVillageTask.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Tasks.Behaviors.VillageTaskBehavior<global::MainCore.Tasks.UpdateVillageTask.Task, global::FluentResults.Result> _villageTaskBehavior;
		private readonly global::MainCore.Tasks.Behaviors.AccountTaskBehavior<global::MainCore.Tasks.UpdateVillageTask.Task, global::FluentResults.Result> _accountTaskBehavior;

		public Handler(
			global::MainCore.Tasks.UpdateVillageTask.HandleBehavior handleBehavior,
			global::MainCore.Tasks.Behaviors.VillageTaskBehavior<global::MainCore.Tasks.UpdateVillageTask.Task, global::FluentResults.Result> villageTaskBehavior,
			global::MainCore.Tasks.Behaviors.AccountTaskBehavior<global::MainCore.Tasks.UpdateVillageTask.Task, global::FluentResults.Result> accountTaskBehavior
		)
		{
			var handlerType = typeof(UpdateVillageTask);

			_handleBehavior = handleBehavior;

			_accountTaskBehavior = accountTaskBehavior;
			_accountTaskBehavior.HandlerType = handlerType;

			_villageTaskBehavior = villageTaskBehavior;
			_villageTaskBehavior.HandlerType = handlerType;

			_villageTaskBehavior.SetInnerHandler(_handleBehavior);
			_accountTaskBehavior.SetInnerHandler(_villageTaskBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Tasks.UpdateVillageTask.Task request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _accountTaskBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Tasks.UpdateVillageTask.Task, global::FluentResults.Result>
	{
		private readonly global::MainCore.Services.ITaskManager _taskManager;
		private readonly global::MainCore.Services.IChromeManager _chromeManager;
		private readonly global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> _contextFactory;
		private readonly global::MainCore.Commands.Update.UpdateBuildingCommand.Handler _updateBuildingCommand;
		private readonly global::MainCore.Commands.Navigate.ToDorfCommand.Handler _toDorfCommand;

		public HandleBehavior(
			global::MainCore.Services.ITaskManager taskManager,
			global::MainCore.Services.IChromeManager chromeManager,
			global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> contextFactory,
			global::MainCore.Commands.Update.UpdateBuildingCommand.Handler updateBuildingCommand,
			global::MainCore.Commands.Navigate.ToDorfCommand.Handler toDorfCommand
		)
		{
			_taskManager = taskManager;
			_chromeManager = chromeManager;
			_contextFactory = contextFactory;
			_updateBuildingCommand = updateBuildingCommand;
			_toDorfCommand = toDorfCommand;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Tasks.UpdateVillageTask.Task request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Tasks.UpdateVillageTask
				.HandleAsync(
					request
					, _taskManager
					, _chromeManager
					, _contextFactory
					, _updateBuildingCommand
					, _toDorfCommand
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
		services.Add(new(typeof(global::MainCore.Tasks.UpdateVillageTask.Handler), typeof(global::MainCore.Tasks.UpdateVillageTask.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Tasks.UpdateVillageTask.Task, global::FluentResults.Result>), typeof(global::MainCore.Tasks.UpdateVillageTask.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Tasks.UpdateVillageTask.HandleBehavior), typeof(global::MainCore.Tasks.UpdateVillageTask.HandleBehavior), lifetime));
		return services;
	}
}
