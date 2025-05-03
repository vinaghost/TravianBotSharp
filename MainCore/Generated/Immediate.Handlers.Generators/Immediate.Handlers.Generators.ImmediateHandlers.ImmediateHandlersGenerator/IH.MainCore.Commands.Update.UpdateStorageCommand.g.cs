using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.Update;

partial class UpdateStorageCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Update.UpdateStorageCommand.Command, global::MainCore.DTO.StorageDto>
	{
		private readonly global::MainCore.Commands.Update.UpdateStorageCommand.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Commands.Update.UpdateStorageCommand.Command, global::MainCore.DTO.StorageDto> _loggingBehavior;

		public Handler(
			global::MainCore.Commands.Update.UpdateStorageCommand.HandleBehavior handleBehavior,
			global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Commands.Update.UpdateStorageCommand.Command, global::MainCore.DTO.StorageDto> loggingBehavior
		)
		{
			var handlerType = typeof(UpdateStorageCommand);

			_handleBehavior = handleBehavior;

			_loggingBehavior = loggingBehavior;
			_loggingBehavior.HandlerType = handlerType;

			_loggingBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::MainCore.DTO.StorageDto> HandleAsync(
			global::MainCore.Commands.Update.UpdateStorageCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _loggingBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.Update.UpdateStorageCommand.Command, global::MainCore.DTO.StorageDto>
	{
		private readonly global::MainCore.Services.IChromeManager _chromeManager;
		private readonly global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> _contextFactory;
		private readonly global::MainCore.Notification.Message.StorageUpdated.Handler _storageUpdated;

		public HandleBehavior(
			global::MainCore.Services.IChromeManager chromeManager,
			global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> contextFactory,
			global::MainCore.Notification.Message.StorageUpdated.Handler storageUpdated
		)
		{
			_chromeManager = chromeManager;
			_contextFactory = contextFactory;
			_storageUpdated = storageUpdated;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::MainCore.DTO.StorageDto> HandleAsync(
			global::MainCore.Commands.Update.UpdateStorageCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Commands.Update.UpdateStorageCommand
				.HandleAsync(
					request
					, _chromeManager
					, _contextFactory
					, _storageUpdated
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
		services.Add(new(typeof(global::MainCore.Commands.Update.UpdateStorageCommand.Handler), typeof(global::MainCore.Commands.Update.UpdateStorageCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Update.UpdateStorageCommand.Command, global::MainCore.DTO.StorageDto>), typeof(global::MainCore.Commands.Update.UpdateStorageCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.Update.UpdateStorageCommand.HandleBehavior), typeof(global::MainCore.Commands.Update.UpdateStorageCommand.HandleBehavior), lifetime));
		return services;
	}
}
