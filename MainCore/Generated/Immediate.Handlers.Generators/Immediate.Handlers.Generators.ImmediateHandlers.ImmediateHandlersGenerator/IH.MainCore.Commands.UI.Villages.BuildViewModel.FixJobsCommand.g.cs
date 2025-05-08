using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.UI.Villages.BuildViewModel;

partial class FixJobsCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.Villages.BuildViewModel.FixJobsCommand.Command, global::System.Collections.Generic.List<global::MainCore.DTO.JobDto>>
	{
		private readonly global::MainCore.Commands.UI.Villages.BuildViewModel.FixJobsCommand.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Commands.UI.Villages.BuildViewModel.FixJobsCommand.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(FixJobsCommand);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.Collections.Generic.List<global::MainCore.DTO.JobDto>> HandleAsync(
			global::MainCore.Commands.UI.Villages.BuildViewModel.FixJobsCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.UI.Villages.BuildViewModel.FixJobsCommand.Command, global::System.Collections.Generic.List<global::MainCore.DTO.JobDto>>
	{
		private readonly global::MainCore.Queries.GetLayoutBuildingsQuery.Handler _getLayoutBuildingsQuery;

		public HandleBehavior(
			global::MainCore.Queries.GetLayoutBuildingsQuery.Handler getLayoutBuildingsQuery
		)
		{
			_getLayoutBuildingsQuery = getLayoutBuildingsQuery;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.Collections.Generic.List<global::MainCore.DTO.JobDto>> HandleAsync(
			global::MainCore.Commands.UI.Villages.BuildViewModel.FixJobsCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Commands.UI.Villages.BuildViewModel.FixJobsCommand
				.HandleAsync(
					request
					, _getLayoutBuildingsQuery
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
		services.Add(new(typeof(global::MainCore.Commands.UI.Villages.BuildViewModel.FixJobsCommand.Handler), typeof(global::MainCore.Commands.UI.Villages.BuildViewModel.FixJobsCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.Villages.BuildViewModel.FixJobsCommand.Command, global::System.Collections.Generic.List<global::MainCore.DTO.JobDto>>), typeof(global::MainCore.Commands.UI.Villages.BuildViewModel.FixJobsCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.UI.Villages.BuildViewModel.FixJobsCommand.HandleBehavior), typeof(global::MainCore.Commands.UI.Villages.BuildViewModel.FixJobsCommand.HandleBehavior), lifetime));
		return services;
	}
}
