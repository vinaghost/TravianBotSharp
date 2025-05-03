using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.UI.MainLayoutViewModel;

partial class LogoutCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.MainLayoutViewModel.LogoutCommand.Command, global::System.ValueTuple>
	{
		private readonly global::MainCore.Commands.UI.MainLayoutViewModel.LogoutCommand.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Commands.UI.MainLayoutViewModel.LogoutCommand.Command, global::System.ValueTuple> _loggingBehavior;

		public Handler(
			global::MainCore.Commands.UI.MainLayoutViewModel.LogoutCommand.HandleBehavior handleBehavior,
			global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Commands.UI.MainLayoutViewModel.LogoutCommand.Command, global::System.ValueTuple> loggingBehavior
		)
		{
			var handlerType = typeof(LogoutCommand);

			_handleBehavior = handleBehavior;

			_loggingBehavior = loggingBehavior;
			_loggingBehavior.HandlerType = handlerType;

			_loggingBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.UI.MainLayoutViewModel.LogoutCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _loggingBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.UI.MainLayoutViewModel.LogoutCommand.Command, global::System.ValueTuple>
	{
		private readonly global::MainCore.Services.IChromeManager _chromeManager;
		private readonly global::MainCore.Services.ITaskManager _taskManager;

		public HandleBehavior(
			global::MainCore.Services.IChromeManager chromeManager,
			global::MainCore.Services.ITaskManager taskManager
		)
		{
			_chromeManager = chromeManager;
			_taskManager = taskManager;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.UI.MainLayoutViewModel.LogoutCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Commands.UI.MainLayoutViewModel.LogoutCommand
				.HandleAsync(
					request
					, _chromeManager
					, _taskManager
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
		services.Add(new(typeof(global::MainCore.Commands.UI.MainLayoutViewModel.LogoutCommand.Handler), typeof(global::MainCore.Commands.UI.MainLayoutViewModel.LogoutCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.MainLayoutViewModel.LogoutCommand.Command, global::System.ValueTuple>), typeof(global::MainCore.Commands.UI.MainLayoutViewModel.LogoutCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.UI.MainLayoutViewModel.LogoutCommand.HandleBehavior), typeof(global::MainCore.Commands.UI.MainLayoutViewModel.LogoutCommand.HandleBehavior), lifetime));
		return services;
	}
}
