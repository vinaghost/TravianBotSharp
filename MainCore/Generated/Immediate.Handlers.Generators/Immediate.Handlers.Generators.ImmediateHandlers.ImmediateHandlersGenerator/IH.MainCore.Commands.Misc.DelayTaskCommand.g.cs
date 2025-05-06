using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.Misc;

partial class DelayTaskCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Misc.DelayTaskCommand.Command, global::System.ValueTuple>
	{
		private readonly global::MainCore.Commands.Misc.DelayTaskCommand.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Commands.Misc.DelayTaskCommand.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(DelayTaskCommand);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.Misc.DelayTaskCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.Misc.DelayTaskCommand.Command, global::System.ValueTuple>
	{
		private readonly global::MainCore.Infrasturecture.Persistence.AppDbContext _context;

		public HandleBehavior(
			global::MainCore.Infrasturecture.Persistence.AppDbContext context
		)
		{
			_context = context;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.Misc.DelayTaskCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Commands.Misc.DelayTaskCommand
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
		services.Add(new(typeof(global::MainCore.Commands.Misc.DelayTaskCommand.Handler), typeof(global::MainCore.Commands.Misc.DelayTaskCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Misc.DelayTaskCommand.Command, global::System.ValueTuple>), typeof(global::MainCore.Commands.Misc.DelayTaskCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.Misc.DelayTaskCommand.HandleBehavior), typeof(global::MainCore.Commands.Misc.DelayTaskCommand.HandleBehavior), lifetime));
		return services;
	}
}
