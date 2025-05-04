using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Notification.Message;

partial class MainWindowUnloaded
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.Message.MainWindowUnloaded.Notification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Message.MainWindowUnloaded.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Notification.Message.MainWindowUnloaded.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(MainWindowUnloaded);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.Message.MainWindowUnloaded.Notification request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Notification.Message.MainWindowUnloaded.Notification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Handlers.MainWindowUnload.ChromeClean.Handler _chromeClean;
		private readonly global::MainCore.Notification.Handlers.MainWindowUnload.ProxyCacheClean.Handler _proxyCacheClean;
		private readonly global::MainCore.Notification.Handlers.MainWindowUnload.UseragentClean.Handler _useragentClean;

		public HandleBehavior(
			global::MainCore.Notification.Handlers.MainWindowUnload.ChromeClean.Handler chromeClean,
			global::MainCore.Notification.Handlers.MainWindowUnload.ProxyCacheClean.Handler proxyCacheClean,
			global::MainCore.Notification.Handlers.MainWindowUnload.UseragentClean.Handler useragentClean
		)
		{
			_chromeClean = chromeClean;
			_proxyCacheClean = proxyCacheClean;
			_useragentClean = useragentClean;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.Message.MainWindowUnloaded.Notification request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Notification.Message.MainWindowUnloaded
				.HandleAsync(
					request
					, _chromeClean
					, _proxyCacheClean
					, _useragentClean
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
		services.Add(new(typeof(global::MainCore.Notification.Message.MainWindowUnloaded.Handler), typeof(global::MainCore.Notification.Message.MainWindowUnloaded.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.Message.MainWindowUnloaded.Notification, global::System.ValueTuple>), typeof(global::MainCore.Notification.Message.MainWindowUnloaded.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Notification.Message.MainWindowUnloaded.HandleBehavior), typeof(global::MainCore.Notification.Message.MainWindowUnloaded.HandleBehavior), lifetime));
		return services;
	}
}
