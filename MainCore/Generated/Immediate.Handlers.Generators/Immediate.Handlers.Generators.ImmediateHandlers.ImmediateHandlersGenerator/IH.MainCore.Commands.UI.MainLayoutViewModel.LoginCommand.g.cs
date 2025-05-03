using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.UI.MainLayoutViewModel;

partial class LoginCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.MainLayoutViewModel.LoginCommand.Command, global::System.ValueTuple>
	{
		private readonly global::MainCore.Commands.UI.MainLayoutViewModel.LoginCommand.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Commands.UI.MainLayoutViewModel.LoginCommand.Command, global::System.ValueTuple> _loggingBehavior;

		public Handler(
			global::MainCore.Commands.UI.MainLayoutViewModel.LoginCommand.HandleBehavior handleBehavior,
			global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Commands.UI.MainLayoutViewModel.LoginCommand.Command, global::System.ValueTuple> loggingBehavior
		)
		{
			var handlerType = typeof(LoginCommand);

			_handleBehavior = handleBehavior;

			_loggingBehavior = loggingBehavior;
			_loggingBehavior.HandlerType = handlerType;

			_loggingBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.UI.MainLayoutViewModel.LoginCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _loggingBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.UI.MainLayoutViewModel.LoginCommand.Command, global::System.ValueTuple>
	{
		private readonly global::MainCore.Services.IChromeManager _chromeManager;
		private readonly global::MainCore.Services.ITaskManager _taskManager;
		private readonly global::MainCore.Services.ITimerManager _timerManager;
		private readonly global::MainCore.Services.ILogService _logService;
		private readonly global::MainCore.Notification.Message.AccountInit.Handler _accountInit;
		private readonly global::MainCore.Commands.Misc.OpenBrowserCommand.Handler _openBrowserCommand;

		public HandleBehavior(
			global::MainCore.Services.IChromeManager chromeManager,
			global::MainCore.Services.ITaskManager taskManager,
			global::MainCore.Services.ITimerManager timerManager,
			global::MainCore.Services.ILogService logService,
			global::MainCore.Notification.Message.AccountInit.Handler accountInit,
			global::MainCore.Commands.Misc.OpenBrowserCommand.Handler openBrowserCommand
		)
		{
			_chromeManager = chromeManager;
			_taskManager = taskManager;
			_timerManager = timerManager;
			_logService = logService;
			_accountInit = accountInit;
			_openBrowserCommand = openBrowserCommand;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.UI.MainLayoutViewModel.LoginCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Commands.UI.MainLayoutViewModel.LoginCommand
				.HandleAsync(
					request
					, _chromeManager
					, _taskManager
					, _timerManager
					, _logService
					, _accountInit
					, _openBrowserCommand
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
		services.Add(new(typeof(global::MainCore.Commands.UI.MainLayoutViewModel.LoginCommand.Handler), typeof(global::MainCore.Commands.UI.MainLayoutViewModel.LoginCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.MainLayoutViewModel.LoginCommand.Command, global::System.ValueTuple>), typeof(global::MainCore.Commands.UI.MainLayoutViewModel.LoginCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.UI.MainLayoutViewModel.LoginCommand.HandleBehavior), typeof(global::MainCore.Commands.UI.MainLayoutViewModel.LoginCommand.HandleBehavior), lifetime));
		return services;
	}
}
