using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.Update;

partial class UpdateStorageCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Update.UpdateStorageCommand.Command, global::MainCore.DTO.StorageDto>
	{
		private readonly global::MainCore.Commands.Update.UpdateStorageCommand.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Commands.Update.UpdateStorageCommand.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(UpdateStorageCommand);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::MainCore.DTO.StorageDto> HandleAsync(
			global::MainCore.Commands.Update.UpdateStorageCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.Update.UpdateStorageCommand.Command, global::MainCore.DTO.StorageDto>
	{
		private readonly global::MainCore.Services.IChromeBrowser _browser;
		private readonly global::MainCore.Infrasturecture.Persistence.AppDbContext _context;

		public HandleBehavior(
			global::MainCore.Services.IChromeBrowser browser,
			global::MainCore.Infrasturecture.Persistence.AppDbContext context
		)
		{
			_browser = browser;
			_context = context;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::MainCore.DTO.StorageDto> HandleAsync(
			global::MainCore.Commands.Update.UpdateStorageCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Commands.Update.UpdateStorageCommand
				.HandleAsync(
					request
					, _browser
					, _context
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
