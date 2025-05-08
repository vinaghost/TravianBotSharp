using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Notifications.Handlers.MainWindowUnload;

partial class UseragentClean
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Constraints.INotification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notifications.Handlers.MainWindowUnload.UseragentClean.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Notifications.Handlers.MainWindowUnload.UseragentClean.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(UseragentClean);

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
		private readonly global::MainCore.Services.IUseragentManager _useragentManager;

		public HandleBehavior(
			global::MainCore.Services.IUseragentManager useragentManager
		)
		{
			_useragentManager = useragentManager;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Constraints.INotification request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Notifications.Handlers.MainWindowUnload.UseragentClean
				.HandleAsync(
					request
					, _useragentManager
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
		services.Add(new(typeof(global::MainCore.Notifications.Handlers.MainWindowUnload.UseragentClean.Handler), typeof(global::MainCore.Notifications.Handlers.MainWindowUnload.UseragentClean.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Constraints.INotification, global::System.ValueTuple>), typeof(global::MainCore.Notifications.Handlers.MainWindowUnload.UseragentClean.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Notifications.Handlers.MainWindowUnload.UseragentClean.HandleBehavior), typeof(global::MainCore.Notifications.Handlers.MainWindowUnload.UseragentClean.HandleBehavior), lifetime));
		return services;
	}
}
