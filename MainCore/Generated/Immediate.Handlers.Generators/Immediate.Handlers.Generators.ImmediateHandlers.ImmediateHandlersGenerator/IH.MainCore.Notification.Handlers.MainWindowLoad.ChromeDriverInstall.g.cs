using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Notification.Handlers.MainWindowLoad;

partial class ChromeDriverInstall
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Constraints.INotification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Handlers.MainWindowLoad.ChromeDriverInstall.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Notification.Handlers.MainWindowLoad.ChromeDriverInstall.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(ChromeDriverInstall);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Constraints.INotification request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Constraints.INotification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Services.IChromeDriverInstaller _chromeDriverInstaller;
		private readonly global::MainCore.UI.ViewModels.UserControls.IWaitingOverlayViewModel _waitingOverlayViewModel;

		public HandleBehavior(
			global::MainCore.Services.IChromeDriverInstaller chromeDriverInstaller,
			global::MainCore.UI.ViewModels.UserControls.IWaitingOverlayViewModel waitingOverlayViewModel
		)
		{
			_chromeDriverInstaller = chromeDriverInstaller;
			_waitingOverlayViewModel = waitingOverlayViewModel;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Constraints.INotification request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Notification.Handlers.MainWindowLoad.ChromeDriverInstall
				.HandleAsync(
					request
					, _chromeDriverInstaller
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
		services.Add(new(typeof(global::MainCore.Notification.Handlers.MainWindowLoad.ChromeDriverInstall.Handler), typeof(global::MainCore.Notification.Handlers.MainWindowLoad.ChromeDriverInstall.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Constraints.INotification, global::System.ValueTuple>), typeof(global::MainCore.Notification.Handlers.MainWindowLoad.ChromeDriverInstall.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Notification.Handlers.MainWindowLoad.ChromeDriverInstall.HandleBehavior), typeof(global::MainCore.Notification.Handlers.MainWindowLoad.ChromeDriverInstall.HandleBehavior), lifetime));
		return services;
	}
}
