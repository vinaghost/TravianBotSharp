using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Notification.Handlers.MainWindowLoad;

partial class DatabaseInstall
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Constraints.INotification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Handlers.MainWindowLoad.DatabaseInstall.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Notification.Handlers.MainWindowLoad.DatabaseInstall.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(DatabaseInstall);

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
		private readonly global::MainCore.Infrasturecture.Persistence.AppDbContext _context;
		private readonly global::MainCore.UI.ViewModels.UserControls.IWaitingOverlayViewModel _waitingOverlayViewModel;

		public HandleBehavior(
			global::MainCore.Infrasturecture.Persistence.AppDbContext context,
			global::MainCore.UI.ViewModels.UserControls.IWaitingOverlayViewModel waitingOverlayViewModel
		)
		{
			_context = context;
			_waitingOverlayViewModel = waitingOverlayViewModel;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Constraints.INotification request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Notification.Handlers.MainWindowLoad.DatabaseInstall
				.HandleAsync(
					request
					, _context
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
		services.Add(new(typeof(global::MainCore.Notification.Handlers.MainWindowLoad.DatabaseInstall.Handler), typeof(global::MainCore.Notification.Handlers.MainWindowLoad.DatabaseInstall.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Constraints.INotification, global::System.ValueTuple>), typeof(global::MainCore.Notification.Handlers.MainWindowLoad.DatabaseInstall.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Notification.Handlers.MainWindowLoad.DatabaseInstall.HandleBehavior), typeof(global::MainCore.Notification.Handlers.MainWindowLoad.DatabaseInstall.HandleBehavior), lifetime));
		return services;
	}
}
