using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.Features.TrainTroop;

partial class TrainTroopCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Features.TrainTroop.TrainTroopCommand.Command, global::FluentResults.Result>
	{
		private readonly global::MainCore.Commands.Features.TrainTroop.TrainTroopCommand.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Commands.Features.TrainTroop.TrainTroopCommand.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(TrainTroopCommand);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Commands.Features.TrainTroop.TrainTroopCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.Features.TrainTroop.TrainTroopCommand.Command, global::FluentResults.Result>
	{
		private readonly global::MainCore.Services.IChromeManager _chromeManager;
		private readonly global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> _contextFactory;
		private readonly global::MainCore.Commands.Navigate.ToDorfCommand.Handler _toDorfCommand;
		private readonly global::MainCore.Commands.Update.UpdateBuildingCommand.Handler _updateBuildingCommand;
		private readonly global::MainCore.Commands.Navigate.ToBuildingCommand.Handler _toBuildingCommand;
		private readonly global::MainCore.Commands.Queries.GetBuildingLocationQuery.Handler _getBuildingLocation;

		public HandleBehavior(
			global::MainCore.Services.IChromeManager chromeManager,
			global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> contextFactory,
			global::MainCore.Commands.Navigate.ToDorfCommand.Handler toDorfCommand,
			global::MainCore.Commands.Update.UpdateBuildingCommand.Handler updateBuildingCommand,
			global::MainCore.Commands.Navigate.ToBuildingCommand.Handler toBuildingCommand,
			global::MainCore.Commands.Queries.GetBuildingLocationQuery.Handler getBuildingLocation
		)
		{
			_chromeManager = chromeManager;
			_contextFactory = contextFactory;
			_toDorfCommand = toDorfCommand;
			_updateBuildingCommand = updateBuildingCommand;
			_toBuildingCommand = toBuildingCommand;
			_getBuildingLocation = getBuildingLocation;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Commands.Features.TrainTroop.TrainTroopCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Commands.Features.TrainTroop.TrainTroopCommand
				.HandleAsync(
					request
					, _chromeManager
					, _contextFactory
					, _toDorfCommand
					, _updateBuildingCommand
					, _toBuildingCommand
					, _getBuildingLocation
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
		services.Add(new(typeof(global::MainCore.Commands.Features.TrainTroop.TrainTroopCommand.Handler), typeof(global::MainCore.Commands.Features.TrainTroop.TrainTroopCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Features.TrainTroop.TrainTroopCommand.Command, global::FluentResults.Result>), typeof(global::MainCore.Commands.Features.TrainTroop.TrainTroopCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.Features.TrainTroop.TrainTroopCommand.HandleBehavior), typeof(global::MainCore.Commands.Features.TrainTroop.TrainTroopCommand.HandleBehavior), lifetime));
		return services;
	}
}
