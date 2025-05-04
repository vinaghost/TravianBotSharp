using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.Features.StartFarmList;

partial class ToFarmListPageCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Features.StartFarmList.ToFarmListPageCommand.Command, global::FluentResults.Result>
	{
		private readonly global::MainCore.Commands.Features.StartFarmList.ToFarmListPageCommand.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Commands.Features.StartFarmList.ToFarmListPageCommand.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(ToFarmListPageCommand);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Commands.Features.StartFarmList.ToFarmListPageCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.Features.StartFarmList.ToFarmListPageCommand.Command, global::FluentResults.Result>
	{
		private readonly global::MainCore.Services.IChromeManager _chromeManager;
		private readonly global::MainCore.Commands.Navigate.SwitchVillageCommand.Handler _switchVillageCommand;
		private readonly global::MainCore.Commands.Navigate.ToDorfCommand.Handler _toDorfCommand;
		private readonly global::MainCore.Commands.Update.UpdateBuildingCommand.Handler _updateBuildingCommand;
		private readonly global::MainCore.Commands.Navigate.ToBuildingCommand.Handler _toBuildingCommand;
		private readonly global::MainCore.Commands.Navigate.SwitchTabCommand.Handler _switchTabCommand;
		private readonly global::MainCore.Commands.Misc.DelayClickCommand.Handler _delayClickCommand;
		private readonly global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> _contextFactory;

		public HandleBehavior(
			global::MainCore.Services.IChromeManager chromeManager,
			global::MainCore.Commands.Navigate.SwitchVillageCommand.Handler switchVillageCommand,
			global::MainCore.Commands.Navigate.ToDorfCommand.Handler toDorfCommand,
			global::MainCore.Commands.Update.UpdateBuildingCommand.Handler updateBuildingCommand,
			global::MainCore.Commands.Navigate.ToBuildingCommand.Handler toBuildingCommand,
			global::MainCore.Commands.Navigate.SwitchTabCommand.Handler switchTabCommand,
			global::MainCore.Commands.Misc.DelayClickCommand.Handler delayClickCommand,
			global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> contextFactory
		)
		{
			_chromeManager = chromeManager;
			_switchVillageCommand = switchVillageCommand;
			_toDorfCommand = toDorfCommand;
			_updateBuildingCommand = updateBuildingCommand;
			_toBuildingCommand = toBuildingCommand;
			_switchTabCommand = switchTabCommand;
			_delayClickCommand = delayClickCommand;
			_contextFactory = contextFactory;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Commands.Features.StartFarmList.ToFarmListPageCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Commands.Features.StartFarmList.ToFarmListPageCommand
				.HandleAsync(
					request
					, _chromeManager
					, _switchVillageCommand
					, _toDorfCommand
					, _updateBuildingCommand
					, _toBuildingCommand
					, _switchTabCommand
					, _delayClickCommand
					, _contextFactory
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
		services.Add(new(typeof(global::MainCore.Commands.Features.StartFarmList.ToFarmListPageCommand.Handler), typeof(global::MainCore.Commands.Features.StartFarmList.ToFarmListPageCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Features.StartFarmList.ToFarmListPageCommand.Command, global::FluentResults.Result>), typeof(global::MainCore.Commands.Features.StartFarmList.ToFarmListPageCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.Features.StartFarmList.ToFarmListPageCommand.HandleBehavior), typeof(global::MainCore.Commands.Features.StartFarmList.ToFarmListPageCommand.HandleBehavior), lifetime));
		return services;
	}
}
