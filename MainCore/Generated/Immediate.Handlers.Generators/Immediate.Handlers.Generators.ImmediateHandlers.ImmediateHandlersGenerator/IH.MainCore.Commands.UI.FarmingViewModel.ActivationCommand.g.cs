using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.UI.FarmingViewModel;

partial class ActivationCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.FarmingViewModel.ActivationCommand.Command, global::System.ValueTuple>
	{
		private readonly global::MainCore.Commands.UI.FarmingViewModel.ActivationCommand.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Commands.UI.FarmingViewModel.ActivationCommand.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(ActivationCommand);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.UI.FarmingViewModel.ActivationCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.UI.FarmingViewModel.ActivationCommand.Command, global::System.ValueTuple>
	{
		private readonly global::MainCore.Infrasturecture.Persistence.AppDbContext _context;
		private readonly global::MainCore.Notification.Message.FarmListUpdated.Handler _farmListUpdated;

		public HandleBehavior(
			global::MainCore.Infrasturecture.Persistence.AppDbContext context,
			global::MainCore.Notification.Message.FarmListUpdated.Handler farmListUpdated
		)
		{
			_context = context;
			_farmListUpdated = farmListUpdated;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.UI.FarmingViewModel.ActivationCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Commands.UI.FarmingViewModel.ActivationCommand
				.HandleAsync(
					request
					, _context
					, _farmListUpdated
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
		services.Add(new(typeof(global::MainCore.Commands.UI.FarmingViewModel.ActivationCommand.Handler), typeof(global::MainCore.Commands.UI.FarmingViewModel.ActivationCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.FarmingViewModel.ActivationCommand.Command, global::System.ValueTuple>), typeof(global::MainCore.Commands.UI.FarmingViewModel.ActivationCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.UI.FarmingViewModel.ActivationCommand.HandleBehavior), typeof(global::MainCore.Commands.UI.FarmingViewModel.ActivationCommand.HandleBehavior), lifetime));
		return services;
	}
}
