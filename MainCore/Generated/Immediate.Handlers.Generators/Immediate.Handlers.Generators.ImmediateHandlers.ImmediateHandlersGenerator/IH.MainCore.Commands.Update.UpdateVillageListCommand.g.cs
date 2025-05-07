using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.Update;

partial class UpdateVillageListCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Update.UpdateVillageListCommand.Command, global::System.ValueTuple>
	{
		private readonly global::MainCore.Commands.Update.UpdateVillageListCommand.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Notification.Behaviors.VillageListUpdatedBehavior<global::MainCore.Commands.Update.UpdateVillageListCommand.Command, global::System.ValueTuple> _villageListUpdatedBehavior;

		public Handler(
			global::MainCore.Commands.Update.UpdateVillageListCommand.HandleBehavior handleBehavior,
			global::MainCore.Notification.Behaviors.VillageListUpdatedBehavior<global::MainCore.Commands.Update.UpdateVillageListCommand.Command, global::System.ValueTuple> villageListUpdatedBehavior
		)
		{
			var handlerType = typeof(UpdateVillageListCommand);

			_handleBehavior = handleBehavior;

			_villageListUpdatedBehavior = villageListUpdatedBehavior;
			_villageListUpdatedBehavior.HandlerType = handlerType;

			_villageListUpdatedBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.Update.UpdateVillageListCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _villageListUpdatedBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.Update.UpdateVillageListCommand.Command, global::System.ValueTuple>
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

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.Update.UpdateVillageListCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Commands.Update.UpdateVillageListCommand
				.HandleAsync(
					request
					, _browser
					, _context
					, cancellationToken
				)
				.ConfigureAwait(false);

			return default;
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public static IServiceCollection AddHandlers(
		IServiceCollection services,
		ServiceLifetime lifetime = ServiceLifetime.Scoped
	)
	{
		services.Add(new(typeof(global::MainCore.Commands.Update.UpdateVillageListCommand.Handler), typeof(global::MainCore.Commands.Update.UpdateVillageListCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Update.UpdateVillageListCommand.Command, global::System.ValueTuple>), typeof(global::MainCore.Commands.Update.UpdateVillageListCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.Update.UpdateVillageListCommand.HandleBehavior), typeof(global::MainCore.Commands.Update.UpdateVillageListCommand.HandleBehavior), lifetime));
		return services;
	}
}
