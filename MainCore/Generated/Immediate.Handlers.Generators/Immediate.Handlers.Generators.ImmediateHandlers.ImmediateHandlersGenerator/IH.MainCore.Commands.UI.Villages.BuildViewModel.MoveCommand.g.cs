using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.UI.Villages.BuildViewModel;

partial class MoveCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.Villages.BuildViewModel.MoveCommand.Command, int>
	{
		private readonly global::MainCore.Commands.UI.Villages.BuildViewModel.MoveCommand.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Commands.UI.Villages.BuildViewModel.MoveCommand.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(MoveCommand);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<int> HandleAsync(
			global::MainCore.Commands.UI.Villages.BuildViewModel.MoveCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.UI.Villages.BuildViewModel.MoveCommand.Command, int>
	{
		private readonly global::MainCore.Infrasturecture.Persistence.AppDbContext _context;
		private readonly global::MainCore.Notification.Message.JobUpdated.Handler _jobUpdated;

		public HandleBehavior(
			global::MainCore.Infrasturecture.Persistence.AppDbContext context,
			global::MainCore.Notification.Message.JobUpdated.Handler jobUpdated
		)
		{
			_context = context;
			_jobUpdated = jobUpdated;
		}

		public override async global::System.Threading.Tasks.ValueTask<int> HandleAsync(
			global::MainCore.Commands.UI.Villages.BuildViewModel.MoveCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Commands.UI.Villages.BuildViewModel.MoveCommand
				.HandleAsync(
					request
					, _context
					, _jobUpdated
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
		services.Add(new(typeof(global::MainCore.Commands.UI.Villages.BuildViewModel.MoveCommand.Handler), typeof(global::MainCore.Commands.UI.Villages.BuildViewModel.MoveCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.Villages.BuildViewModel.MoveCommand.Command, int>), typeof(global::MainCore.Commands.UI.Villages.BuildViewModel.MoveCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.UI.Villages.BuildViewModel.MoveCommand.HandleBehavior), typeof(global::MainCore.Commands.UI.Villages.BuildViewModel.MoveCommand.HandleBehavior), lifetime));
		return services;
	}
}
