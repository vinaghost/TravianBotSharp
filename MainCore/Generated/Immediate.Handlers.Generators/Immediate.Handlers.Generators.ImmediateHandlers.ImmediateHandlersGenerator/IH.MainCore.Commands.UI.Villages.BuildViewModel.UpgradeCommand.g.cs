using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.UI.Villages.BuildViewModel;

partial class UpgradeCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.Villages.BuildViewModel.UpgradeCommand.Command, global::System.ValueTuple>
	{
		private readonly global::MainCore.Commands.UI.Villages.BuildViewModel.UpgradeCommand.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Commands.UI.Villages.BuildViewModel.UpgradeCommand.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(UpgradeCommand);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.UI.Villages.BuildViewModel.UpgradeCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.UI.Villages.BuildViewModel.UpgradeCommand.Command, global::System.ValueTuple>
	{
		private readonly global::MainCore.Commands.Misc.AddJobCommand.Handler _addJobCommand;
		private readonly global::MainCore.Commands.Queries.GetLayoutBuildingsQuery.Handler _getLayoutBuildingsQuery;

		public HandleBehavior(
			global::MainCore.Commands.Misc.AddJobCommand.Handler addJobCommand,
			global::MainCore.Commands.Queries.GetLayoutBuildingsQuery.Handler getLayoutBuildingsQuery
		)
		{
			_addJobCommand = addJobCommand;
			_getLayoutBuildingsQuery = getLayoutBuildingsQuery;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.UI.Villages.BuildViewModel.UpgradeCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Commands.UI.Villages.BuildViewModel.UpgradeCommand
				.HandleAsync(
					request
					, _addJobCommand
					, _getLayoutBuildingsQuery
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
		services.Add(new(typeof(global::MainCore.Commands.UI.Villages.BuildViewModel.UpgradeCommand.Handler), typeof(global::MainCore.Commands.UI.Villages.BuildViewModel.UpgradeCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.Villages.BuildViewModel.UpgradeCommand.Command, global::System.ValueTuple>), typeof(global::MainCore.Commands.UI.Villages.BuildViewModel.UpgradeCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.UI.Villages.BuildViewModel.UpgradeCommand.HandleBehavior), typeof(global::MainCore.Commands.UI.Villages.BuildViewModel.UpgradeCommand.HandleBehavior), lifetime));
		return services;
	}
}
