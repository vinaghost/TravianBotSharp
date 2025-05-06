using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.Features.TrainTroop;

partial class TrainTroopCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Features.TrainTroop.TrainTroopCommand.Command, global::FluentResults.Result>
	{
		private readonly global::MainCore.Commands.Features.TrainTroop.TrainTroopCommand.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Commands.Behaviors.CommandLoggingBehavior<global::MainCore.Commands.Features.TrainTroop.TrainTroopCommand.Command, global::FluentResults.Result> _commandLoggingBehavior;

		public Handler(
			global::MainCore.Commands.Features.TrainTroop.TrainTroopCommand.HandleBehavior handleBehavior,
			global::MainCore.Commands.Behaviors.CommandLoggingBehavior<global::MainCore.Commands.Features.TrainTroop.TrainTroopCommand.Command, global::FluentResults.Result> commandLoggingBehavior
		)
		{
			var handlerType = typeof(TrainTroopCommand);

			_handleBehavior = handleBehavior;

			_commandLoggingBehavior = commandLoggingBehavior;
			_commandLoggingBehavior.HandlerType = handlerType;

			_commandLoggingBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Commands.Features.TrainTroop.TrainTroopCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _commandLoggingBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.Features.TrainTroop.TrainTroopCommand.Command, global::FluentResults.Result>
	{
		private readonly global::MainCore.Services.IChromeBrowser _browser;
		private readonly global::MainCore.Infrasturecture.Persistence.AppDbContext _context;
		private readonly global::MainCore.Commands.Navigate.ToDorfCommand.Handler _toDorfCommand;
		private readonly global::MainCore.Commands.Update.UpdateBuildingCommand.Handler _updateBuildingCommand;
		private readonly global::MainCore.Commands.Navigate.ToBuildingCommand.Handler _toBuildingCommand;
		private readonly global::MainCore.Queries.GetBuildingLocationQuery.Handler _getBuildingLocation;

		public HandleBehavior(
			global::MainCore.Services.IChromeBrowser browser,
			global::MainCore.Infrasturecture.Persistence.AppDbContext context,
			global::MainCore.Commands.Navigate.ToDorfCommand.Handler toDorfCommand,
			global::MainCore.Commands.Update.UpdateBuildingCommand.Handler updateBuildingCommand,
			global::MainCore.Commands.Navigate.ToBuildingCommand.Handler toBuildingCommand,
			global::MainCore.Queries.GetBuildingLocationQuery.Handler getBuildingLocation
		)
		{
			_browser = browser;
			_context = context;
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
					, _browser
					, _context
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
