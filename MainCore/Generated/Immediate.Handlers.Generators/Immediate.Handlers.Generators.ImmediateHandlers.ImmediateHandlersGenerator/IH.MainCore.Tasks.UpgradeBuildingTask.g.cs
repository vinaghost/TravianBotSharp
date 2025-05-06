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
		private readonly global::MainCore.Commands.Behaviors.CommandLoggingBehavior<global::MainCore.Tasks.UpgradeBuildingTask.Task, global::FluentResults.Result> _commandLoggingBehavior;

		public Handler(
			global::MainCore.Tasks.UpgradeBuildingTask.HandleBehavior handleBehavior,
			global::MainCore.Tasks.Behaviors.VillageTaskBehavior<global::MainCore.Tasks.UpgradeBuildingTask.Task, global::FluentResults.Result> villageTaskBehavior,
			global::MainCore.Tasks.Behaviors.AccountTaskBehavior<global::MainCore.Tasks.UpgradeBuildingTask.Task, global::FluentResults.Result> accountTaskBehavior,
			global::MainCore.Commands.Behaviors.CommandLoggingBehavior<global::MainCore.Tasks.UpgradeBuildingTask.Task, global::FluentResults.Result> commandLoggingBehavior
		)
		{
			var handlerType = typeof(UpgradeBuildingTask);

			_handleBehavior = handleBehavior;

			_commandLoggingBehavior = commandLoggingBehavior;
			_commandLoggingBehavior.HandlerType = handlerType;

			_accountTaskBehavior = accountTaskBehavior;
			_accountTaskBehavior.HandlerType = handlerType;

			_villageTaskBehavior = villageTaskBehavior;
			_villageTaskBehavior.HandlerType = handlerType;

			_villageTaskBehavior.SetInnerHandler(_handleBehavior);
			_accountTaskBehavior.SetInnerHandler(_villageTaskBehavior);
			_commandLoggingBehavior.SetInnerHandler(_accountTaskBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Tasks.UpgradeBuildingTask.Task request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _commandLoggingBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Tasks.UpgradeBuildingTask.Task, global::FluentResults.Result>
	{
		private readonly global::Serilog.ILogger _logger;
		private readonly global::MainCore.Commands.Features.UpgradeBuilding.HandleJobCommand.Handler _handleJobCommand;
		private readonly global::MainCore.Commands.Features.UpgradeBuilding.ToBuildPageCommand.Handler _toBuildPageCommand;
		private readonly global::MainCore.Commands.Features.UpgradeBuilding.HandleResourceCommand.Handler _handleResourceCommand;
		private readonly global::MainCore.Commands.Features.UpgradeBuilding.HandleUpgradeCommand.Handler _handleUpgradeCommand;
		private readonly global::MainCore.Queries.GetFirstQueueBuildingQuery.Handler _getFirstQueueBuildingQuery;

		public HandleBehavior(
			global::Serilog.ILogger logger,
			global::MainCore.Commands.Features.UpgradeBuilding.HandleJobCommand.Handler handleJobCommand,
			global::MainCore.Commands.Features.UpgradeBuilding.ToBuildPageCommand.Handler toBuildPageCommand,
			global::MainCore.Commands.Features.UpgradeBuilding.HandleResourceCommand.Handler handleResourceCommand,
			global::MainCore.Commands.Features.UpgradeBuilding.HandleUpgradeCommand.Handler handleUpgradeCommand,
			global::MainCore.Queries.GetFirstQueueBuildingQuery.Handler getFirstQueueBuildingQuery
		)
		{
			_logger = logger;
			_handleJobCommand = handleJobCommand;
			_toBuildPageCommand = toBuildPageCommand;
			_handleResourceCommand = handleResourceCommand;
			_handleUpgradeCommand = handleUpgradeCommand;
			_getFirstQueueBuildingQuery = getFirstQueueBuildingQuery;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Tasks.UpgradeBuildingTask.Task request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Tasks.UpgradeBuildingTask
				.HandleAsync(
					request
					, _logger
					, _handleJobCommand
					, _toBuildPageCommand
					, _handleResourceCommand
					, _handleUpgradeCommand
					, _getFirstQueueBuildingQuery
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
