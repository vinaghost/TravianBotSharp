using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Notification.Message;

partial class MainWindowLoaded
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.Message.MainWindowLoaded.Notification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Message.MainWindowLoaded.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Notification.Message.MainWindowLoaded.Notification, global::System.ValueTuple> _loggingBehavior;

		public Handler(
			global::MainCore.Notification.Message.MainWindowLoaded.HandleBehavior handleBehavior,
			global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Notification.Message.MainWindowLoaded.Notification, global::System.ValueTuple> loggingBehavior
		)
		{
			var handlerType = typeof(MainWindowLoaded);

			_handleBehavior = handleBehavior;

			_loggingBehavior = loggingBehavior;
			_loggingBehavior.HandlerType = handlerType;

			_loggingBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.Message.MainWindowLoaded.Notification request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _loggingBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Notification.Message.MainWindowLoaded.Notification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Handlers.MainWindowLoad.ChromeDriverInstall.Handler _chromeDriverInstall;
		private readonly global::MainCore.Notification.Handlers.MainWindowLoad.ChromeExtensionInstall.Handler _chromeExtensionInstall;
		private readonly global::MainCore.Notification.Handlers.MainWindowLoad.ChromeUserAgentInstall.Handler _chromeUserAgentInstall;
		private readonly global::MainCore.Notification.Handlers.MainWindowLoad.DatabaseInstall.Handler _databaseInstall;

		public HandleBehavior(
			global::MainCore.Notification.Handlers.MainWindowLoad.ChromeDriverInstall.Handler chromeDriverInstall,
			global::MainCore.Notification.Handlers.MainWindowLoad.ChromeExtensionInstall.Handler chromeExtensionInstall,
			global::MainCore.Notification.Handlers.MainWindowLoad.ChromeUserAgentInstall.Handler chromeUserAgentInstall,
			global::MainCore.Notification.Handlers.MainWindowLoad.DatabaseInstall.Handler databaseInstall
		)
		{
			_chromeDriverInstall = chromeDriverInstall;
			_chromeExtensionInstall = chromeExtensionInstall;
			_chromeUserAgentInstall = chromeUserAgentInstall;
			_databaseInstall = databaseInstall;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.Message.MainWindowLoaded.Notification request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Notification.Message.MainWindowLoaded
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
		services.Add(new(typeof(global::MainCore.Notification.Message.MainWindowLoaded.Handler), typeof(global::MainCore.Notification.Message.MainWindowLoaded.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.Message.MainWindowLoaded.Notification, global::System.ValueTuple>), typeof(global::MainCore.Notification.Message.MainWindowLoaded.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Notification.Message.MainWindowLoaded.HandleBehavior), typeof(global::MainCore.Notification.Message.MainWindowLoaded.HandleBehavior), lifetime));
		return services;
	}
}
