using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Notification.Handlers.Refresh;

partial class AccountListRefresh
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Constraints.INotification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Handlers.Refresh.AccountListRefresh.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Notification.Handlers.Refresh.AccountListRefresh.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(AccountListRefresh);

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
		private readonly global::MainCore.UI.ViewModels.UserControls.MainLayoutViewModel _mainLayoutViewModel;

		public HandleBehavior(
			global::MainCore.UI.ViewModels.UserControls.MainLayoutViewModel mainLayoutViewModel
		)
		{
			_mainLayoutViewModel = mainLayoutViewModel;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Constraints.INotification request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Notification.Handlers.Refresh.AccountListRefresh
				.HandleAsync(
					request
					, _mainLayoutViewModel
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
		services.Add(new(typeof(global::MainCore.Notification.Handlers.Refresh.AccountListRefresh.Handler), typeof(global::MainCore.Notification.Handlers.Refresh.AccountListRefresh.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Constraints.INotification, global::System.ValueTuple>), typeof(global::MainCore.Notification.Handlers.Refresh.AccountListRefresh.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Notification.Handlers.Refresh.AccountListRefresh.HandleBehavior), typeof(global::MainCore.Notification.Handlers.Refresh.AccountListRefresh.HandleBehavior), lifetime));
		return services;
	}
}
