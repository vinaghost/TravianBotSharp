using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Notification.Handlers.Refresh;

partial class VillageListRefresh
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Constraints.IAccountNotification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Handlers.Refresh.VillageListRefresh.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Notification.Handlers.Refresh.VillageListRefresh.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(VillageListRefresh);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Constraints.IAccountNotification request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Constraints.IAccountNotification, global::System.ValueTuple>
	{
		private readonly global::MainCore.UI.ViewModels.Tabs.VillageViewModel _viewModel;

		public HandleBehavior(
			global::MainCore.UI.ViewModels.Tabs.VillageViewModel viewModel
		)
		{
			_viewModel = viewModel;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Constraints.IAccountNotification request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Notification.Handlers.Refresh.VillageListRefresh
				.HandleAsync(
					request
					, _viewModel
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
		services.Add(new(typeof(global::MainCore.Notification.Handlers.Refresh.VillageListRefresh.Handler), typeof(global::MainCore.Notification.Handlers.Refresh.VillageListRefresh.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Constraints.IAccountNotification, global::System.ValueTuple>), typeof(global::MainCore.Notification.Handlers.Refresh.VillageListRefresh.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Notification.Handlers.Refresh.VillageListRefresh.HandleBehavior), typeof(global::MainCore.Notification.Handlers.Refresh.VillageListRefresh.HandleBehavior), lifetime));
		return services;
	}
}
