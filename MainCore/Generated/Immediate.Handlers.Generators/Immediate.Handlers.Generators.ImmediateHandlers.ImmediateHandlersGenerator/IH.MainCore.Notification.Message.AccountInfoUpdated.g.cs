using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Notification.Message;

partial class AccountInfoUpdated
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.Message.AccountInfoUpdated.Notification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Message.AccountInfoUpdated.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Notification.Message.AccountInfoUpdated.Notification, global::System.ValueTuple> _loggingBehavior;

		public Handler(
			global::MainCore.Notification.Message.AccountInfoUpdated.HandleBehavior handleBehavior,
			global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Notification.Message.AccountInfoUpdated.Notification, global::System.ValueTuple> loggingBehavior
		)
		{
			var handlerType = typeof(AccountInfoUpdated);

			_handleBehavior = handleBehavior;

			_loggingBehavior = loggingBehavior;
			_loggingBehavior.HandlerType = handlerType;

			_loggingBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.Message.AccountInfoUpdated.Notification request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _loggingBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Notification.Message.AccountInfoUpdated.Notification, global::System.ValueTuple>
	{

		public HandleBehavior(
		)
		{
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.Message.AccountInfoUpdated.Notification request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Notification.Message.AccountInfoUpdated
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
		services.Add(new(typeof(global::MainCore.Notification.Message.AccountInfoUpdated.Handler), typeof(global::MainCore.Notification.Message.AccountInfoUpdated.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.Message.AccountInfoUpdated.Notification, global::System.ValueTuple>), typeof(global::MainCore.Notification.Message.AccountInfoUpdated.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Notification.Message.AccountInfoUpdated.HandleBehavior), typeof(global::MainCore.Notification.Message.AccountInfoUpdated.HandleBehavior), lifetime));
		return services;
	}
}
