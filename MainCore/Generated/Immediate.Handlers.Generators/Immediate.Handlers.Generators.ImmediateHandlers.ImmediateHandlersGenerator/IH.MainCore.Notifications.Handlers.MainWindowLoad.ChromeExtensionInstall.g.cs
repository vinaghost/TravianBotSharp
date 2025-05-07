using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Notifications.Handlers.MainWindowLoad;

partial class ChromeExtensionInstall
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Constraints.INotification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notifications.Handlers.MainWindowLoad.ChromeExtensionInstall.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Notifications.Handlers.MainWindowLoad.ChromeExtensionInstall.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(ChromeExtensionInstall);

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
		private readonly global::MainCore.Services.IChromeManager _chromeManager;
		private readonly global::MainCore.UI.ViewModels.UserControls.IWaitingOverlayViewModel _waitingOverlayViewModel;

		public HandleBehavior(
			global::MainCore.Services.IChromeManager chromeManager,
			global::MainCore.UI.ViewModels.UserControls.IWaitingOverlayViewModel waitingOverlayViewModel
		)
		{
			_chromeManager = chromeManager;
			_waitingOverlayViewModel = waitingOverlayViewModel;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Constraints.INotification request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Notifications.Handlers.MainWindowLoad.ChromeExtensionInstall
				.HandleAsync(
					request
					, _chromeManager
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
		services.Add(new(typeof(global::MainCore.Notifications.Handlers.MainWindowLoad.ChromeExtensionInstall.Handler), typeof(global::MainCore.Notifications.Handlers.MainWindowLoad.ChromeExtensionInstall.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Constraints.INotification, global::System.ValueTuple>), typeof(global::MainCore.Notifications.Handlers.MainWindowLoad.ChromeExtensionInstall.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Notifications.Handlers.MainWindowLoad.ChromeExtensionInstall.HandleBehavior), typeof(global::MainCore.Notifications.Handlers.MainWindowLoad.ChromeExtensionInstall.HandleBehavior), lifetime));
		return services;
	}
}
