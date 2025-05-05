using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.UI.Villages.BuildViewModel;

partial class ImportCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.Villages.BuildViewModel.ImportCommand.Command, global::System.ValueTuple>
	{
		private readonly global::MainCore.Commands.UI.Villages.BuildViewModel.ImportCommand.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Commands.UI.Villages.BuildViewModel.ImportCommand.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(ImportCommand);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.UI.Villages.BuildViewModel.ImportCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.UI.Villages.BuildViewModel.ImportCommand.Command, global::System.ValueTuple>
	{
		private readonly global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> _contextFactory;
		private readonly global::MainCore.Queries.GetLayoutBuildingsQuery.Handler _getLayoutBuildingsQuery;
		private readonly global::MainCore.Notification.Message.JobUpdated.Handler _jobUpdated;

		public HandleBehavior(
			global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> contextFactory,
			global::MainCore.Queries.GetLayoutBuildingsQuery.Handler getLayoutBuildingsQuery,
			global::MainCore.Notification.Message.JobUpdated.Handler jobUpdated
		)
		{
			_contextFactory = contextFactory;
			_getLayoutBuildingsQuery = getLayoutBuildingsQuery;
			_jobUpdated = jobUpdated;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.UI.Villages.BuildViewModel.ImportCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Commands.UI.Villages.BuildViewModel.ImportCommand
				.HandleAsync(
					request
					, _contextFactory
					, _getLayoutBuildingsQuery
					, _jobUpdated
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
		services.Add(new(typeof(global::MainCore.Commands.UI.Villages.BuildViewModel.ImportCommand.Handler), typeof(global::MainCore.Commands.UI.Villages.BuildViewModel.ImportCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.Villages.BuildViewModel.ImportCommand.Command, global::System.ValueTuple>), typeof(global::MainCore.Commands.UI.Villages.BuildViewModel.ImportCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.UI.Villages.BuildViewModel.ImportCommand.HandleBehavior), typeof(global::MainCore.Commands.UI.Villages.BuildViewModel.ImportCommand.HandleBehavior), lifetime));
		return services;
	}
}
