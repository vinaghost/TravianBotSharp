using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.Features.UpgradeBuilding;

partial class HandleResourceCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Features.UpgradeBuilding.HandleResourceCommand.Command, global::FluentResults.Result>
	{
		private readonly global::MainCore.Commands.Features.UpgradeBuilding.HandleResourceCommand.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Commands.Features.UpgradeBuilding.HandleResourceCommand.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(HandleResourceCommand);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Commands.Features.UpgradeBuilding.HandleResourceCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.Features.UpgradeBuilding.HandleResourceCommand.Command, global::FluentResults.Result>
	{
		private readonly global::MainCore.Notification.Message.JobUpdated.Handler _jobUpdated;
		private readonly global::MainCore.Commands.Update.UpdateStorageCommand.Handler _updateStorageCommand;
		private readonly global::MainCore.Commands.Features.UseHeroItem.UseHeroResourceCommand.Handler _useHeroResourceCommand;
		private readonly global::MainCore.Commands.Features.UseHeroItem.ToHeroInventoryCommand.Handler _toHeroInventoryCommand;
		private readonly global::MainCore.Commands.Update.UpdateInventoryCommand.Handler _updateInventoryCommand;
		private readonly global::MainCore.Commands.Misc.AddJobCommand.Handler _addJobCommand;
		private readonly global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> _contextFactory;
		private readonly global::MainCore.Services.IChromeManager _chromeManager;
		private readonly global::MainCore.Services.ILogService _logService;

		public HandleBehavior(
			global::MainCore.Notification.Message.JobUpdated.Handler jobUpdated,
			global::MainCore.Commands.Update.UpdateStorageCommand.Handler updateStorageCommand,
			global::MainCore.Commands.Features.UseHeroItem.UseHeroResourceCommand.Handler useHeroResourceCommand,
			global::MainCore.Commands.Features.UseHeroItem.ToHeroInventoryCommand.Handler toHeroInventoryCommand,
			global::MainCore.Commands.Update.UpdateInventoryCommand.Handler updateInventoryCommand,
			global::MainCore.Commands.Misc.AddJobCommand.Handler addJobCommand,
			global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> contextFactory,
			global::MainCore.Services.IChromeManager chromeManager,
			global::MainCore.Services.ILogService logService
		)
		{
			_jobUpdated = jobUpdated;
			_updateStorageCommand = updateStorageCommand;
			_useHeroResourceCommand = useHeroResourceCommand;
			_toHeroInventoryCommand = toHeroInventoryCommand;
			_updateInventoryCommand = updateInventoryCommand;
			_addJobCommand = addJobCommand;
			_contextFactory = contextFactory;
			_chromeManager = chromeManager;
			_logService = logService;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Commands.Features.UpgradeBuilding.HandleResourceCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Commands.Features.UpgradeBuilding.HandleResourceCommand
				.HandleAsync(
					request
					, _jobUpdated
					, _updateStorageCommand
					, _useHeroResourceCommand
					, _toHeroInventoryCommand
					, _updateInventoryCommand
					, _addJobCommand
					, _contextFactory
					, _chromeManager
					, _logService
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
		services.Add(new(typeof(global::MainCore.Commands.Features.UpgradeBuilding.HandleResourceCommand.Handler), typeof(global::MainCore.Commands.Features.UpgradeBuilding.HandleResourceCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Features.UpgradeBuilding.HandleResourceCommand.Command, global::FluentResults.Result>), typeof(global::MainCore.Commands.Features.UpgradeBuilding.HandleResourceCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.Features.UpgradeBuilding.HandleResourceCommand.HandleBehavior), typeof(global::MainCore.Commands.Features.UpgradeBuilding.HandleResourceCommand.HandleBehavior), lifetime));
		return services;
	}
}
