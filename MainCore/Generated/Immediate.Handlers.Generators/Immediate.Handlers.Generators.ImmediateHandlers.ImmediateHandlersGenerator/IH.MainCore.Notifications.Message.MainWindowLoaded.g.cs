using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Notifications.Message;

partial class MainWindowLoaded
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notifications.Message.MainWindowLoaded.Notification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notifications.Message.MainWindowLoaded.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Notifications.Message.MainWindowLoaded.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(MainWindowLoaded);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notifications.Message.MainWindowLoaded.Notification request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Notifications.Message.MainWindowLoaded.Notification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notifications.Handlers.MainWindowLoad.ChromeDriverInstall.Handler _chromeDriverInstall;
		private readonly global::MainCore.Notifications.Handlers.MainWindowLoad.ChromeExtensionInstall.Handler _chromeExtensionInstall;
		private readonly global::MainCore.Notifications.Handlers.MainWindowLoad.ChromeUserAgentInstall.Handler _chromeUserAgentInstall;
		private readonly global::MainCore.Notifications.Handlers.MainWindowLoad.DatabaseInstall.Handler _databaseInstall;

		public HandleBehavior(
			global::MainCore.Notifications.Handlers.MainWindowLoad.ChromeDriverInstall.Handler chromeDriverInstall,
			global::MainCore.Notifications.Handlers.MainWindowLoad.ChromeExtensionInstall.Handler chromeExtensionInstall,
			global::MainCore.Notifications.Handlers.MainWindowLoad.ChromeUserAgentInstall.Handler chromeUserAgentInstall,
			global::MainCore.Notifications.Handlers.MainWindowLoad.DatabaseInstall.Handler databaseInstall
		)
		{
			_chromeDriverInstall = chromeDriverInstall;
			_chromeExtensionInstall = chromeExtensionInstall;
			_chromeUserAgentInstall = chromeUserAgentInstall;
			_databaseInstall = databaseInstall;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notifications.Message.MainWindowLoaded.Notification request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Notifications.Message.MainWindowLoaded
				.HandleAsync(
					request
					, _chromeDriverInstall
					, _chromeExtensionInstall
					, _chromeUserAgentInstall
					, _databaseInstall
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
		services.Add(new(typeof(global::MainCore.Notifications.Message.MainWindowLoaded.Handler), typeof(global::MainCore.Notifications.Message.MainWindowLoaded.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notifications.Message.MainWindowLoaded.Notification, global::System.ValueTuple>), typeof(global::MainCore.Notifications.Message.MainWindowLoaded.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Notifications.Message.MainWindowLoaded.HandleBehavior), typeof(global::MainCore.Notifications.Message.MainWindowLoaded.HandleBehavior), lifetime));
		return services;
	}
}
