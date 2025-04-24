using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Notification.Message;

partial class AccountLogout
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.Message.AccountLogout.Notification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Message.AccountLogout.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Notification.Message.AccountLogout.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(AccountLogout);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.Message.AccountLogout.Notification request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Notification.Message.AccountLogout.Notification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Handlers.Trigger.LoginTaskTrigger.Handler _loginTaskTrigger;

		public HandleBehavior(
			global::MainCore.Notification.Handlers.Trigger.LoginTaskTrigger.Handler loginTaskTrigger
		)
		{
			_loginTaskTrigger = loginTaskTrigger;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.Message.AccountLogout.Notification request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Notification.Message.AccountLogout
				.HandleAsync(
					request
					, _loginTaskTrigger
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
		services.Add(new(typeof(global::MainCore.Notification.Message.AccountLogout.Handler), typeof(global::MainCore.Notification.Message.AccountLogout.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.Message.AccountLogout.Notification, global::System.ValueTuple>), typeof(global::MainCore.Notification.Message.AccountLogout.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Notification.Message.AccountLogout.HandleBehavior), typeof(global::MainCore.Notification.Message.AccountLogout.HandleBehavior), lifetime));
		return services;
	}
}
