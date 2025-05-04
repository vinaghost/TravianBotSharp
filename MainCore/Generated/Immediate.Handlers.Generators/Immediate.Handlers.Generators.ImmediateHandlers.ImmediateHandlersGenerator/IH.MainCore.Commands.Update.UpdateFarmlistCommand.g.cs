using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.Update;

partial class UpdateFarmlistCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Update.UpdateFarmlistCommand.Command, global::FluentResults.Result>
	{
		private readonly global::MainCore.Commands.Update.UpdateFarmlistCommand.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Commands.Update.UpdateFarmlistCommand.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(UpdateFarmlistCommand);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Commands.Update.UpdateFarmlistCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.Update.UpdateFarmlistCommand.Command, global::FluentResults.Result>
	{
		private readonly global::MainCore.Services.IChromeManager _chromeManager;
		private readonly global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> _contextFactory;
		private readonly global::MainCore.Notification.Message.FarmListUpdated.Handler _farmListUpdated;

		public HandleBehavior(
			global::MainCore.Services.IChromeManager chromeManager,
			global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> contextFactory,
			global::MainCore.Notification.Message.FarmListUpdated.Handler farmListUpdated
		)
		{
			_chromeManager = chromeManager;
			_contextFactory = contextFactory;
			_farmListUpdated = farmListUpdated;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Commands.Update.UpdateFarmlistCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Commands.Update.UpdateFarmlistCommand
				.HandleAsync(
					request
					, _chromeManager
					, _contextFactory
					, _farmListUpdated
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
		services.Add(new(typeof(global::MainCore.Commands.Update.UpdateFarmlistCommand.Handler), typeof(global::MainCore.Commands.Update.UpdateFarmlistCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Update.UpdateFarmlistCommand.Command, global::FluentResults.Result>), typeof(global::MainCore.Commands.Update.UpdateFarmlistCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.Update.UpdateFarmlistCommand.HandleBehavior), typeof(global::MainCore.Commands.Update.UpdateFarmlistCommand.HandleBehavior), lifetime));
		return services;
	}
}
