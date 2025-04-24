using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Notification.Handlers.MainWindowUnload;

partial class ProxyCacheClean
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.Message.MainWindowUnloaded, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Handlers.MainWindowUnload.ProxyCacheClean.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Notification.Handlers.MainWindowUnload.ProxyCacheClean.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(ProxyCacheClean);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.Message.MainWindowUnloaded request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Notification.Message.MainWindowUnloaded, global::System.ValueTuple>
	{

		public HandleBehavior(
		)
		{
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.Message.MainWindowUnloaded request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Notification.Handlers.MainWindowUnload.ProxyCacheClean
				.HandleAsync(
					request
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
		services.Add(new(typeof(global::MainCore.Notification.Handlers.MainWindowUnload.ProxyCacheClean.Handler), typeof(global::MainCore.Notification.Handlers.MainWindowUnload.ProxyCacheClean.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.Message.MainWindowUnloaded, global::System.ValueTuple>), typeof(global::MainCore.Notification.Handlers.MainWindowUnload.ProxyCacheClean.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Notification.Handlers.MainWindowUnload.ProxyCacheClean.HandleBehavior), typeof(global::MainCore.Notification.Handlers.MainWindowUnload.ProxyCacheClean.HandleBehavior), lifetime));
		return services;
	}
}
