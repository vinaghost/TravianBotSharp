using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Notifications.Handlers.Trigger;

partial class ChangeWallTrigger
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Constraints.IAccountVillageConstraint, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notifications.Handlers.Trigger.ChangeWallTrigger.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Notifications.Handlers.Trigger.ChangeWallTrigger.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(ChangeWallTrigger);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Constraints.IAccountVillageConstraint request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Constraints.IAccountVillageConstraint, global::System.ValueTuple>
	{
		private readonly global::MainCore.Infrasturecture.Persistence.AppDbContext _context;

		public HandleBehavior(
			global::MainCore.Infrasturecture.Persistence.AppDbContext context
		)
		{
			_context = context;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Constraints.IAccountVillageConstraint request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Notifications.Handlers.Trigger.ChangeWallTrigger
				.HandleAsync(
					request
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
		services.Add(new(typeof(global::MainCore.Notifications.Handlers.Trigger.ChangeWallTrigger.Handler), typeof(global::MainCore.Notifications.Handlers.Trigger.ChangeWallTrigger.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Constraints.IAccountVillageConstraint, global::System.ValueTuple>), typeof(global::MainCore.Notifications.Handlers.Trigger.ChangeWallTrigger.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Notifications.Handlers.Trigger.ChangeWallTrigger.HandleBehavior), typeof(global::MainCore.Notifications.Handlers.Trigger.ChangeWallTrigger.HandleBehavior), lifetime));
		return services;
	}
}
