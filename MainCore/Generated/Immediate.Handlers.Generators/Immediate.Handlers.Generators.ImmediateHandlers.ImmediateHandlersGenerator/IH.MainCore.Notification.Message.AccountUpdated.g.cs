using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Notification.Message;

partial class AccountUpdated
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.Message.AccountUpdated.Notification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Message.AccountUpdated.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Notification.Message.AccountUpdated.Notification, global::System.ValueTuple> _loggingBehavior;

		public Handler(
			global::MainCore.Notification.Message.AccountUpdated.HandleBehavior handleBehavior,
			global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Notification.Message.AccountUpdated.Notification, global::System.ValueTuple> loggingBehavior
		)
		{
			var handlerType = typeof(AccountUpdated);

			_handleBehavior = handleBehavior;

			_loggingBehavior = loggingBehavior;
			_loggingBehavior.HandlerType = handlerType;

			_loggingBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.Message.AccountUpdated.Notification request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _loggingBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Notification.Message.AccountUpdated.Notification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Handlers.Refresh.AccountListRefresh.Handler _accountListRefresh;

		public HandleBehavior(
			global::MainCore.Notification.Handlers.Refresh.AccountListRefresh.Handler accountListRefresh
		)
		{
			_accountListRefresh = accountListRefresh;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.Message.AccountUpdated.Notification request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Notification.Message.AccountUpdated
				.HandleAsync(
					request
					, _accountListRefresh
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
		services.Add(new(typeof(global::MainCore.Notification.Message.AccountUpdated.Handler), typeof(global::MainCore.Notification.Message.AccountUpdated.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.Message.AccountUpdated.Notification, global::System.ValueTuple>), typeof(global::MainCore.Notification.Message.AccountUpdated.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Notification.Message.AccountUpdated.HandleBehavior), typeof(global::MainCore.Notification.Message.AccountUpdated.HandleBehavior), lifetime));
		return services;
	}
}
