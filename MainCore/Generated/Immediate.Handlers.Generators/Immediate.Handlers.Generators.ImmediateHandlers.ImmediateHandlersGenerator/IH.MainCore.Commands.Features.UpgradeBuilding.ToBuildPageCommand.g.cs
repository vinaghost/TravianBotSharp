using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.Features.UpgradeBuilding;

partial class ToBuildPageCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Features.UpgradeBuilding.ToBuildPageCommand.Command, global::FluentResults.Result>
	{
		private readonly global::MainCore.Commands.Features.UpgradeBuilding.ToBuildPageCommand.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Commands.Features.UpgradeBuilding.ToBuildPageCommand.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(ToBuildPageCommand);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Commands.Features.UpgradeBuilding.ToBuildPageCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.Features.UpgradeBuilding.ToBuildPageCommand.Command, global::FluentResults.Result>
	{
		private readonly global::MainCore.Commands.Navigate.ToBuildingCommand.Handler _toBuildingCommand;
		private readonly global::MainCore.Commands.Navigate.SwitchTabCommand.Handler _switchTabCommand;
		private readonly global::MainCore.Commands.Queries.GetBuildingQuery.Handler _getBuilding;

		public HandleBehavior(
			global::MainCore.Commands.Navigate.ToBuildingCommand.Handler toBuildingCommand,
			global::MainCore.Commands.Navigate.SwitchTabCommand.Handler switchTabCommand,
			global::MainCore.Commands.Queries.GetBuildingQuery.Handler getBuilding
		)
		{
			_toBuildingCommand = toBuildingCommand;
			_switchTabCommand = switchTabCommand;
			_getBuilding = getBuilding;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Commands.Features.UpgradeBuilding.ToBuildPageCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Commands.Features.UpgradeBuilding.ToBuildPageCommand
				.HandleAsync(
					request
					, _toBuildingCommand
					, _switchTabCommand
					, _getBuilding
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
		services.Add(new(typeof(global::MainCore.Commands.Features.UpgradeBuilding.ToBuildPageCommand.Handler), typeof(global::MainCore.Commands.Features.UpgradeBuilding.ToBuildPageCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Features.UpgradeBuilding.ToBuildPageCommand.Command, global::FluentResults.Result>), typeof(global::MainCore.Commands.Features.UpgradeBuilding.ToBuildPageCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.Features.UpgradeBuilding.ToBuildPageCommand.HandleBehavior), typeof(global::MainCore.Commands.Features.UpgradeBuilding.ToBuildPageCommand.HandleBehavior), lifetime));
		return services;
	}
}
