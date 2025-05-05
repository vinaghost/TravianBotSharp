using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Notification.Handlers.MainWindowLoad;

partial class ChromeUserAgentInstall
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.Base.INotification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Handlers.MainWindowLoad.ChromeUserAgentInstall.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Notification.Handlers.MainWindowLoad.ChromeUserAgentInstall.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(ChromeUserAgentInstall);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.Base.INotification request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Notification.Base.INotification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Services.IUseragentManager _useragentManager;
		private readonly global::MainCore.UI.ViewModels.UserControls.IWaitingOverlayViewModel _waitingOverlayViewModel;

		public HandleBehavior(
			global::MainCore.Services.IUseragentManager useragentManager,
			global::MainCore.UI.ViewModels.UserControls.IWaitingOverlayViewModel waitingOverlayViewModel
		)
		{
			_useragentManager = useragentManager;
			_waitingOverlayViewModel = waitingOverlayViewModel;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.Base.INotification request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Notification.Handlers.MainWindowLoad.ChromeUserAgentInstall
				.HandleAsync(
					request
					, _useragentManager
					, _waitingOverlayViewModel
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
		services.Add(new(typeof(global::MainCore.Notification.Handlers.MainWindowLoad.ChromeUserAgentInstall.Handler), typeof(global::MainCore.Notification.Handlers.MainWindowLoad.ChromeUserAgentInstall.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.Base.INotification, global::System.ValueTuple>), typeof(global::MainCore.Notification.Handlers.MainWindowLoad.ChromeUserAgentInstall.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Notification.Handlers.MainWindowLoad.ChromeUserAgentInstall.HandleBehavior), typeof(global::MainCore.Notification.Handlers.MainWindowLoad.ChromeUserAgentInstall.HandleBehavior), lifetime));
		return services;
	}
}
