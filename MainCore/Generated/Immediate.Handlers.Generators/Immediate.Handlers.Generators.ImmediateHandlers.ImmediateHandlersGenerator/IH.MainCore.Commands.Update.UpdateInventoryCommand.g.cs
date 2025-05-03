using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.Update;

partial class UpdateInventoryCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Update.UpdateInventoryCommand.Command, global::FluentResults.Result>
	{
		private readonly global::MainCore.Commands.Update.UpdateInventoryCommand.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Commands.Update.UpdateInventoryCommand.Command, global::FluentResults.Result> _loggingBehavior;

		public Handler(
			global::MainCore.Commands.Update.UpdateInventoryCommand.HandleBehavior handleBehavior,
			global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Commands.Update.UpdateInventoryCommand.Command, global::FluentResults.Result> loggingBehavior
		)
		{
			var handlerType = typeof(UpdateInventoryCommand);

			_handleBehavior = handleBehavior;

			_loggingBehavior = loggingBehavior;
			_loggingBehavior.HandlerType = handlerType;

			_loggingBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Commands.Update.UpdateInventoryCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _loggingBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.Update.UpdateInventoryCommand.Command, global::FluentResults.Result>
	{
		private readonly global::MainCore.Services.IChromeManager _chromeManager;
		private readonly global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> _contextFactory;
		private readonly global::MainCore.Notification.Message.HeroItemUpdated.Handler _heroItemUpdated;

		public HandleBehavior(
			global::MainCore.Services.IChromeManager chromeManager,
			global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> contextFactory,
			global::MainCore.Notification.Message.HeroItemUpdated.Handler heroItemUpdated
		)
		{
			_chromeManager = chromeManager;
			_contextFactory = contextFactory;
			_heroItemUpdated = heroItemUpdated;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Commands.Update.UpdateInventoryCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Commands.Update.UpdateInventoryCommand
				.HandleAsync(
					request
					, _chromeManager
					, _contextFactory
					, _heroItemUpdated
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
		services.Add(new(typeof(global::MainCore.Commands.Update.UpdateInventoryCommand.Handler), typeof(global::MainCore.Commands.Update.UpdateInventoryCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Update.UpdateInventoryCommand.Command, global::FluentResults.Result>), typeof(global::MainCore.Commands.Update.UpdateInventoryCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.Update.UpdateInventoryCommand.HandleBehavior), typeof(global::MainCore.Commands.Update.UpdateInventoryCommand.HandleBehavior), lifetime));
		return services;
	}
}
