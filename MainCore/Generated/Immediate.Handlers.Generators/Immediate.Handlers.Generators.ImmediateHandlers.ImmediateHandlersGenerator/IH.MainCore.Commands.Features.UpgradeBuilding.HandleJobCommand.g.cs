using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.Features.UpgradeBuilding;

partial class HandleJobCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Features.UpgradeBuilding.HandleJobCommand.Command, global::FluentResults.Result<global::MainCore.Models.NormalBuildPlan>>
	{
		private readonly global::MainCore.Commands.Features.UpgradeBuilding.HandleJobCommand.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Commands.Features.UpgradeBuilding.HandleJobCommand.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(HandleJobCommand);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result<global::MainCore.Models.NormalBuildPlan>> HandleAsync(
			global::MainCore.Commands.Features.UpgradeBuilding.HandleJobCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.Features.UpgradeBuilding.HandleJobCommand.Command, global::FluentResults.Result<global::MainCore.Models.NormalBuildPlan>>
	{
		private readonly global::MainCore.Queries.GetJobQuery.Handler _getJobQuery;
		private readonly global::MainCore.Commands.Navigate.ToDorfCommand.Handler _toDorfCommand;
		private readonly global::MainCore.Commands.Update.UpdateBuildingCommand.Handler _updateBuildingCommand;
		private readonly global::MainCore.Queries.GetLayoutBuildingsQuery.Handler _getLayoutBuildingsQuery;
		private readonly global::MainCore.Commands.Misc.DeleteJobByIdCommand.Handler _deleteJobByIdCommand;
		private readonly global::MainCore.Commands.Misc.AddJobCommand.Handler _addJobCommand;
		private readonly global::MainCore.Notification.Message.JobUpdated.Handler _jobUpdated;

		public HandleBehavior(
			global::MainCore.Queries.GetJobQuery.Handler getJobQuery,
			global::MainCore.Commands.Navigate.ToDorfCommand.Handler toDorfCommand,
			global::MainCore.Commands.Update.UpdateBuildingCommand.Handler updateBuildingCommand,
			global::MainCore.Queries.GetLayoutBuildingsQuery.Handler getLayoutBuildingsQuery,
			global::MainCore.Commands.Misc.DeleteJobByIdCommand.Handler deleteJobByIdCommand,
			global::MainCore.Commands.Misc.AddJobCommand.Handler addJobCommand,
			global::MainCore.Notification.Message.JobUpdated.Handler jobUpdated
		)
		{
			_getJobQuery = getJobQuery;
			_toDorfCommand = toDorfCommand;
			_updateBuildingCommand = updateBuildingCommand;
			_getLayoutBuildingsQuery = getLayoutBuildingsQuery;
			_deleteJobByIdCommand = deleteJobByIdCommand;
			_addJobCommand = addJobCommand;
			_jobUpdated = jobUpdated;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result<global::MainCore.Models.NormalBuildPlan>> HandleAsync(
			global::MainCore.Commands.Features.UpgradeBuilding.HandleJobCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Commands.Features.UpgradeBuilding.HandleJobCommand
				.HandleAsync(
					request
					, _getJobQuery
					, _toDorfCommand
					, _updateBuildingCommand
					, _getLayoutBuildingsQuery
					, _deleteJobByIdCommand
					, _addJobCommand
					, _jobUpdated
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
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Features.UpgradeBuilding.HandleJobCommand.Command, global::FluentResults.Result<global::MainCore.Models.NormalBuildPlan>>), typeof(global::MainCore.Commands.Features.UpgradeBuilding.HandleJobCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.Features.UpgradeBuilding.HandleJobCommand.HandleBehavior), typeof(global::MainCore.Commands.Features.UpgradeBuilding.HandleJobCommand.HandleBehavior), lifetime));
		return services;
	}
}
