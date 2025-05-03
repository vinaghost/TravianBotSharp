using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Notification.Message;

partial class AccountSettingUpdated
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.Message.AccountSettingUpdated.Notification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Message.AccountSettingUpdated.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Notification.Message.AccountSettingUpdated.Notification, global::System.ValueTuple> _loggingBehavior;

		public Handler(
			global::MainCore.Notification.Message.AccountSettingUpdated.HandleBehavior handleBehavior,
			global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Notification.Message.AccountSettingUpdated.Notification, global::System.ValueTuple> loggingBehavior
		)
		{
			var handlerType = typeof(AccountSettingUpdated);

			_handleBehavior = handleBehavior;

			_loggingBehavior = loggingBehavior;
			_loggingBehavior.HandlerType = handlerType;

			_loggingBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.Message.AccountSettingUpdated.Notification request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _loggingBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Notification.Message.AccountSettingUpdated.Notification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Handlers.Trigger.StartAdventureTaskTrigger.Handler _startAdventureTaskTrigger;
		private readonly global::MainCore.Notification.Handlers.Refresh.AccountSettingRefresh.Handler _accountSettingRefresh;

		public HandleBehavior(
			global::MainCore.Notification.Handlers.Trigger.StartAdventureTaskTrigger.Handler startAdventureTaskTrigger,
			global::MainCore.Notification.Handlers.Refresh.AccountSettingRefresh.Handler accountSettingRefresh
		)
		{
			_startAdventureTaskTrigger = startAdventureTaskTrigger;
			_accountSettingRefresh = accountSettingRefresh;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.Message.AccountSettingUpdated.Notification request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Notification.Message.AccountSettingUpdated
				.HandleAsync(
					request
					, _startAdventureTaskTrigger
					, _accountSettingRefresh
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
		services.Add(new(typeof(global::MainCore.Notification.Message.AccountSettingUpdated.Handler), typeof(global::MainCore.Notification.Message.AccountSettingUpdated.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.Message.AccountSettingUpdated.Notification, global::System.ValueTuple>), typeof(global::MainCore.Notification.Message.AccountSettingUpdated.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Notification.Message.AccountSettingUpdated.HandleBehavior), typeof(global::MainCore.Notification.Message.AccountSettingUpdated.HandleBehavior), lifetime));
		return services;
	}
}
