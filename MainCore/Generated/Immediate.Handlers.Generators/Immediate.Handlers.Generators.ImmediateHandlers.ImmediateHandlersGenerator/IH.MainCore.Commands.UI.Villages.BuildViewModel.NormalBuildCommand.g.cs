using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.UI.Villages.BuildViewModel;

partial class NormalBuildCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.Villages.BuildViewModel.NormalBuildCommand.Command, global::System.ValueTuple>
	{
		private readonly global::MainCore.Commands.UI.Villages.BuildViewModel.NormalBuildCommand.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Commands.UI.Villages.BuildViewModel.NormalBuildCommand.Command, global::System.ValueTuple> _loggingBehavior;

		public Handler(
			global::MainCore.Commands.UI.Villages.BuildViewModel.NormalBuildCommand.HandleBehavior handleBehavior,
			global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Commands.UI.Villages.BuildViewModel.NormalBuildCommand.Command, global::System.ValueTuple> loggingBehavior
		)
		{
			var handlerType = typeof(NormalBuildCommand);

			_handleBehavior = handleBehavior;

			_loggingBehavior = loggingBehavior;
			_loggingBehavior.HandlerType = handlerType;

			_loggingBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.UI.Villages.BuildViewModel.NormalBuildCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _loggingBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.UI.Villages.BuildViewModel.NormalBuildCommand.Command, global::System.ValueTuple>
	{
		private readonly global::MainCore.Services.IDialogService _dialogService;
		private readonly global::MainCore.Commands.Queries.GetLayoutBuildingsQuery.Handler _getLayoutBuildingsQuery;
		private readonly global::MainCore.Commands.Misc.AddJobCommand.Handler _addJobCommand;

		public HandleBehavior(
			global::MainCore.Services.IDialogService dialogService,
			global::MainCore.Commands.Queries.GetLayoutBuildingsQuery.Handler getLayoutBuildingsQuery,
			global::MainCore.Commands.Misc.AddJobCommand.Handler addJobCommand
		)
		{
			_dialogService = dialogService;
			_getLayoutBuildingsQuery = getLayoutBuildingsQuery;
			_addJobCommand = addJobCommand;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.UI.Villages.BuildViewModel.NormalBuildCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Commands.UI.Villages.BuildViewModel.NormalBuildCommand
				.HandleAsync(
					request
					, _dialogService
					, _getLayoutBuildingsQuery
					, _addJobCommand
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
		services.Add(new(typeof(global::MainCore.Commands.UI.Villages.BuildViewModel.NormalBuildCommand.Handler), typeof(global::MainCore.Commands.UI.Villages.BuildViewModel.NormalBuildCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.Villages.BuildViewModel.NormalBuildCommand.Command, global::System.ValueTuple>), typeof(global::MainCore.Commands.UI.Villages.BuildViewModel.NormalBuildCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.UI.Villages.BuildViewModel.NormalBuildCommand.HandleBehavior), typeof(global::MainCore.Commands.UI.Villages.BuildViewModel.NormalBuildCommand.HandleBehavior), lifetime));
		return services;
	}
}
