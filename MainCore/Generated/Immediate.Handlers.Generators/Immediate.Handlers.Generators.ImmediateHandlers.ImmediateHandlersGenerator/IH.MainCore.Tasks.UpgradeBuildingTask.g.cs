using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Tasks;

partial class UpgradeBuildingTask
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Tasks.UpgradeBuildingTask.Task, global::FluentResults.Result>
	{
		private readonly global::MainCore.Tasks.UpgradeBuildingTask.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Tasks.Behaviors.VillageTaskBehavior<global::MainCore.Tasks.UpgradeBuildingTask.Task, global::FluentResults.Result> _villageTaskBehavior;
		private readonly global::MainCore.Tasks.Behaviors.AccountTaskBehavior<global::MainCore.Tasks.UpgradeBuildingTask.Task, global::FluentResults.Result> _accountTaskBehavior;

		public Handler(
			global::MainCore.Tasks.UpgradeBuildingTask.HandleBehavior handleBehavior,
			global::MainCore.Tasks.Behaviors.VillageTaskBehavior<global::MainCore.Tasks.UpgradeBuildingTask.Task, global::FluentResults.Result> villageTaskBehavior,
			global::MainCore.Tasks.Behaviors.AccountTaskBehavior<global::MainCore.Tasks.UpgradeBuildingTask.Task, global::FluentResults.Result> accountTaskBehavior
		)
		{
			var handlerType = typeof(UpgradeBuildingTask);

			_handleBehavior = handleBehavior;

			_accountTaskBehavior = accountTaskBehavior;
			_accountTaskBehavior.HandlerType = handlerType;

			_villageTaskBehavior = villageTaskBehavior;
			_villageTaskBehavior.HandlerType = handlerType;

			_villageTaskBehavior.SetInnerHandler(_handleBehavior);
			_accountTaskBehavior.SetInnerHandler(_villageTaskBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Tasks.UpgradeBuildingTask.Task request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _accountTaskBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Tasks.UpgradeBuildingTask.Task, global::FluentResults.Result>
	{
		private readonly global::MainCore.Services.ITaskManager _taskManager;
		private readonly global::MainCore.Services.IChromeManager _chromeManager;
		private readonly global::MainCore.Services.ILogService _logService;
		private readonly global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> _contextFactory;
		private readonly global::MainCore.Commands.Features.UpgradeBuilding.HandleJobCommand.Handler _handleJobCommand;
		private readonly global::MainCore.Commands.Features.UpgradeBuilding.ToBuildPageCommand.Handler _toBuildPageCommand;
		private readonly global::MainCore.Commands.Features.UpgradeBuilding.HandleResourceCommand.Handler _handleResourceCommand;
		private readonly global::MainCore.Commands.Features.UpgradeBuilding.HandleUpgradeCommand.Handler _handleUpgradeCommand;

		public HandleBehavior(
			global::MainCore.Services.ITaskManager taskManager,
			global::MainCore.Services.IChromeManager chromeManager,
			global::MainCore.Services.ILogService logService,
			global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> contextFactory,
			global::MainCore.Commands.Features.UpgradeBuilding.HandleJobCommand.Handler handleJobCommand,
			global::MainCore.Commands.Features.UpgradeBuilding.ToBuildPageCommand.Handler toBuildPageCommand,
			global::MainCore.Commands.Features.UpgradeBuilding.HandleResourceCommand.Handler handleResourceCommand,
			global::MainCore.Commands.Features.UpgradeBuilding.HandleUpgradeCommand.Handler handleUpgradeCommand
		)
		{
			_taskManager = taskManager;
			_chromeManager = chromeManager;
			_logService = logService;
			_contextFactory = contextFactory;
			_handleJobCommand = handleJobCommand;
			_toBuildPageCommand = toBuildPageCommand;
			_handleResourceCommand = handleResourceCommand;
			_handleUpgradeCommand = handleUpgradeCommand;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Tasks.UpgradeBuildingTask.Task request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Tasks.UpgradeBuildingTask
				.HandleAsync(
					request
					, _taskManager
					, _chromeManager
					, _logService
					, _contextFactory
					, _handleJobCommand
					, _toBuildPageCommand
					, _handleResourceCommand
					, _handleUpgradeCommand
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
		services.Add(new(typeof(global::MainCore.Tasks.UpgradeBuildingTask.Handler), typeof(global::MainCore.Tasks.UpgradeBuildingTask.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Tasks.UpgradeBuildingTask.Task, global::FluentResults.Result>), typeof(global::MainCore.Tasks.UpgradeBuildingTask.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Tasks.UpgradeBuildingTask.HandleBehavior), typeof(global::MainCore.Tasks.UpgradeBuildingTask.HandleBehavior), lifetime));
		return services;
	}
}
