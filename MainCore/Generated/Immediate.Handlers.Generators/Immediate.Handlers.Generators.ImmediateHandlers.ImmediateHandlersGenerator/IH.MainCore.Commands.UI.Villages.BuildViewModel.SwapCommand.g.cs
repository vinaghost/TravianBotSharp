using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.UI.Villages.BuildViewModel;

partial class SwapCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.Villages.BuildViewModel.SwapCommand.Command, int>
	{
		private readonly global::MainCore.Commands.UI.Villages.BuildViewModel.SwapCommand.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Commands.UI.Villages.BuildViewModel.SwapCommand.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(SwapCommand);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<int> HandleAsync(
			global::MainCore.Commands.UI.Villages.BuildViewModel.SwapCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.UI.Villages.BuildViewModel.SwapCommand.Command, int>
	{
		private readonly global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> _contextFactory;
		private readonly global::MainCore.Notification.Message.JobUpdated.Handler _jobUpdated;

		public HandleBehavior(
			global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> contextFactory,
			global::MainCore.Notification.Message.JobUpdated.Handler jobUpdated
		)
		{
			_contextFactory = contextFactory;
			_jobUpdated = jobUpdated;
		}

		public override async global::System.Threading.Tasks.ValueTask<int> HandleAsync(
			global::MainCore.Commands.UI.Villages.BuildViewModel.SwapCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Commands.UI.Villages.BuildViewModel.SwapCommand
				.HandleAsync(
					request
					, _contextFactory
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
		services.Add(new(typeof(global::MainCore.Commands.UI.Villages.BuildViewModel.SwapCommand.Handler), typeof(global::MainCore.Commands.UI.Villages.BuildViewModel.SwapCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.Villages.BuildViewModel.SwapCommand.Command, int>), typeof(global::MainCore.Commands.UI.Villages.BuildViewModel.SwapCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.UI.Villages.BuildViewModel.SwapCommand.HandleBehavior), typeof(global::MainCore.Commands.UI.Villages.BuildViewModel.SwapCommand.HandleBehavior), lifetime));
		return services;
	}
}
