using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.UI.Villages.BuildViewModel;

partial class NormalBuildCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.Villages.BuildViewModel.NormalBuildCommand.Command, global::FluentResults.Result>
	{
		private readonly global::MainCore.Commands.UI.Villages.BuildViewModel.NormalBuildCommand.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Commands.Behaviors.CommandLoggingBehavior<global::MainCore.Commands.UI.Villages.BuildViewModel.NormalBuildCommand.Command, global::FluentResults.Result> _commandLoggingBehavior;

		public Handler(
			global::MainCore.Commands.UI.Villages.BuildViewModel.NormalBuildCommand.HandleBehavior handleBehavior,
			global::MainCore.Commands.Behaviors.CommandLoggingBehavior<global::MainCore.Commands.UI.Villages.BuildViewModel.NormalBuildCommand.Command, global::FluentResults.Result> commandLoggingBehavior
		)
		{
			var handlerType = typeof(NormalBuildCommand);

			_handleBehavior = handleBehavior;

			_commandLoggingBehavior = commandLoggingBehavior;
			_commandLoggingBehavior.HandlerType = handlerType;

			_commandLoggingBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Commands.UI.Villages.BuildViewModel.NormalBuildCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _commandLoggingBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.UI.Villages.BuildViewModel.NormalBuildCommand.Command, global::FluentResults.Result>
	{
		private readonly global::MainCore.Queries.GetLayoutBuildingsQuery.Handler _getLayoutBuildingsQuery;
		private readonly global::MainCore.Commands.Misc.AddJobCommand.Handler _addJobCommand;

		public HandleBehavior(
			global::MainCore.Queries.GetLayoutBuildingsQuery.Handler getLayoutBuildingsQuery,
			global::MainCore.Commands.Misc.AddJobCommand.Handler addJobCommand
		)
		{
			_getLayoutBuildingsQuery = getLayoutBuildingsQuery;
			_addJobCommand = addJobCommand;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Commands.UI.Villages.BuildViewModel.NormalBuildCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Commands.UI.Villages.BuildViewModel.NormalBuildCommand
				.HandleAsync(
					request
					, _getLayoutBuildingsQuery
					, _addJobCommand
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
		services.Add(new(typeof(global::MainCore.Commands.UI.Villages.BuildViewModel.NormalBuildCommand.Handler), typeof(global::MainCore.Commands.UI.Villages.BuildViewModel.NormalBuildCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.Villages.BuildViewModel.NormalBuildCommand.Command, global::FluentResults.Result>), typeof(global::MainCore.Commands.UI.Villages.BuildViewModel.NormalBuildCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.UI.Villages.BuildViewModel.NormalBuildCommand.HandleBehavior), typeof(global::MainCore.Commands.UI.Villages.BuildViewModel.NormalBuildCommand.HandleBehavior), lifetime));
		return services;
	}
}
