using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.Features.NpcResource;

partial class ToNpcResourcePageCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Features.NpcResource.ToNpcResourcePageCommand.Command, global::FluentResults.Result>
	{
		private readonly global::MainCore.Commands.Features.NpcResource.ToNpcResourcePageCommand.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Commands.Behaviors.CommandLoggingBehavior<global::MainCore.Commands.Features.NpcResource.ToNpcResourcePageCommand.Command, global::FluentResults.Result> _commandLoggingBehavior;

		public Handler(
			global::MainCore.Commands.Features.NpcResource.ToNpcResourcePageCommand.HandleBehavior handleBehavior,
			global::MainCore.Commands.Behaviors.CommandLoggingBehavior<global::MainCore.Commands.Features.NpcResource.ToNpcResourcePageCommand.Command, global::FluentResults.Result> commandLoggingBehavior
		)
		{
			var handlerType = typeof(ToNpcResourcePageCommand);

			_handleBehavior = handleBehavior;

			_commandLoggingBehavior = commandLoggingBehavior;
			_commandLoggingBehavior.HandlerType = handlerType;

			_commandLoggingBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Commands.Features.NpcResource.ToNpcResourcePageCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _commandLoggingBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.Features.NpcResource.ToNpcResourcePageCommand.Command, global::FluentResults.Result>
	{
		private readonly global::MainCore.Services.IChromeBrowser _browser;
		private readonly global::MainCore.Commands.Navigate.ToDorfCommand.Handler _toDorfCommand;
		private readonly global::MainCore.Commands.Update.UpdateBuildingCommand.Handler _updateBuildingCommand;
		private readonly global::MainCore.Commands.Navigate.ToBuildingCommand.Handler _toBuildingCommand;
		private readonly global::MainCore.Commands.Navigate.SwitchTabCommand.Handler _switchTabCommand;
		private readonly global::MainCore.Queries.GetBuildingLocationQuery.Handler _getBuildingLocation;

		public HandleBehavior(
			global::MainCore.Services.IChromeBrowser browser,
			global::MainCore.Commands.Navigate.ToDorfCommand.Handler toDorfCommand,
			global::MainCore.Commands.Update.UpdateBuildingCommand.Handler updateBuildingCommand,
			global::MainCore.Commands.Navigate.ToBuildingCommand.Handler toBuildingCommand,
			global::MainCore.Commands.Navigate.SwitchTabCommand.Handler switchTabCommand,
			global::MainCore.Queries.GetBuildingLocationQuery.Handler getBuildingLocation
		)
		{
			_browser = browser;
			_toDorfCommand = toDorfCommand;
			_updateBuildingCommand = updateBuildingCommand;
			_toBuildingCommand = toBuildingCommand;
			_switchTabCommand = switchTabCommand;
			_getBuildingLocation = getBuildingLocation;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Commands.Features.NpcResource.ToNpcResourcePageCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Commands.Features.NpcResource.ToNpcResourcePageCommand
				.HandleAsync(
					request
					, _browser
					, _toDorfCommand
					, _updateBuildingCommand
					, _toBuildingCommand
					, _switchTabCommand
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
		services.Add(new(typeof(global::MainCore.Commands.Features.NpcResource.ToNpcResourcePageCommand.Handler), typeof(global::MainCore.Commands.Features.NpcResource.ToNpcResourcePageCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Features.NpcResource.ToNpcResourcePageCommand.Command, global::FluentResults.Result>), typeof(global::MainCore.Commands.Features.NpcResource.ToNpcResourcePageCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.Features.NpcResource.ToNpcResourcePageCommand.HandleBehavior), typeof(global::MainCore.Commands.Features.NpcResource.ToNpcResourcePageCommand.HandleBehavior), lifetime));
		return services;
	}
}
