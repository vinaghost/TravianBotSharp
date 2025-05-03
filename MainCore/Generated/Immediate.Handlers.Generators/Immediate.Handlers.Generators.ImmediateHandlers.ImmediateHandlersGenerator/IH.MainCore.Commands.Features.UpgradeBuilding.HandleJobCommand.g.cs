using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.Features.UpgradeBuilding;

partial class HandleJobCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Features.UpgradeBuilding.HandleJobCommand.Command, global::FluentResults.Result<global::MainCore.Common.Models.NormalBuildPlan>>
	{
		private readonly global::MainCore.Commands.Features.UpgradeBuilding.HandleJobCommand.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Commands.Features.UpgradeBuilding.HandleJobCommand.Command, global::FluentResults.Result<global::MainCore.Common.Models.NormalBuildPlan>> _loggingBehavior;

		public Handler(
			global::MainCore.Commands.Features.UpgradeBuilding.HandleJobCommand.HandleBehavior handleBehavior,
			global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Commands.Features.UpgradeBuilding.HandleJobCommand.Command, global::FluentResults.Result<global::MainCore.Common.Models.NormalBuildPlan>> loggingBehavior
		)
		{
			var handlerType = typeof(HandleJobCommand);

			_handleBehavior = handleBehavior;

			_loggingBehavior = loggingBehavior;
			_loggingBehavior.HandlerType = handlerType;

			_loggingBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result<global::MainCore.Common.Models.NormalBuildPlan>> HandleAsync(
			global::MainCore.Commands.Features.UpgradeBuilding.HandleJobCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _loggingBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.Features.UpgradeBuilding.HandleJobCommand.Command, global::FluentResults.Result<global::MainCore.Common.Models.NormalBuildPlan>>
	{
		private readonly global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> _contextFactory;
		private readonly global::MainCore.Commands.Navigate.ToDorfCommand.Handler _toDorfCommand;
		private readonly global::MainCore.Commands.Update.UpdateBuildingCommand.Handler _updateBuildingCommand;
		private readonly global::MainCore.Commands.Queries.GetLayoutBuildingsQuery.Handler _getLayoutBuildingsQuery;
		private readonly global::MainCore.Commands.Misc.DeleteJobByIdCommand.Handler _deleteJobByIdCommand;
		private readonly global::MainCore.Commands.Misc.AddJobCommand.Handler _addJobCommand;

		public HandleBehavior(
			global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> contextFactory,
			global::MainCore.Commands.Navigate.ToDorfCommand.Handler toDorfCommand,
			global::MainCore.Commands.Update.UpdateBuildingCommand.Handler updateBuildingCommand,
			global::MainCore.Commands.Queries.GetLayoutBuildingsQuery.Handler getLayoutBuildingsQuery,
			global::MainCore.Commands.Misc.DeleteJobByIdCommand.Handler deleteJobByIdCommand,
			global::MainCore.Commands.Misc.AddJobCommand.Handler addJobCommand
		)
		{
			_contextFactory = contextFactory;
			_toDorfCommand = toDorfCommand;
			_updateBuildingCommand = updateBuildingCommand;
			_getLayoutBuildingsQuery = getLayoutBuildingsQuery;
			_deleteJobByIdCommand = deleteJobByIdCommand;
			_addJobCommand = addJobCommand;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result<global::MainCore.Common.Models.NormalBuildPlan>> HandleAsync(
			global::MainCore.Commands.Features.UpgradeBuilding.HandleJobCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Commands.Features.UpgradeBuilding.HandleJobCommand
				.HandleAsync(
					request
					, _contextFactory
					, _toDorfCommand
					, _updateBuildingCommand
					, _getLayoutBuildingsQuery
					, _deleteJobByIdCommand
					, _addJobCommand
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
		services.Add(new(typeof(global::MainCore.Commands.Features.UpgradeBuilding.HandleJobCommand.Handler), typeof(global::MainCore.Commands.Features.UpgradeBuilding.HandleJobCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Features.UpgradeBuilding.HandleJobCommand.Command, global::FluentResults.Result<global::MainCore.Common.Models.NormalBuildPlan>>), typeof(global::MainCore.Commands.Features.UpgradeBuilding.HandleJobCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.Features.UpgradeBuilding.HandleJobCommand.HandleBehavior), typeof(global::MainCore.Commands.Features.UpgradeBuilding.HandleJobCommand.HandleBehavior), lifetime));
		return services;
	}
}
