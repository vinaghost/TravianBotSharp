using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Notification.Message;

partial class QuestUpdated
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.Message.QuestUpdated.Notification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Message.QuestUpdated.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Notification.Message.QuestUpdated.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(QuestUpdated);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.Message.QuestUpdated.Notification request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Notification.Message.QuestUpdated.Notification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Handlers.Trigger.ClaimQuestTaskTrigger.Handler _claimQuestTaskTrigger;

		public HandleBehavior(
			global::MainCore.Notification.Handlers.Trigger.ClaimQuestTaskTrigger.Handler claimQuestTaskTrigger
		)
		{
			_claimQuestTaskTrigger = claimQuestTaskTrigger;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.Message.QuestUpdated.Notification request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Notification.Message.QuestUpdated
				.HandleAsync(
					request
					, _claimQuestTaskTrigger
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
		services.Add(new(typeof(global::MainCore.Notification.Message.QuestUpdated.Handler), typeof(global::MainCore.Notification.Message.QuestUpdated.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.Message.QuestUpdated.Notification, global::System.ValueTuple>), typeof(global::MainCore.Notification.Message.QuestUpdated.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Notification.Message.QuestUpdated.HandleBehavior), typeof(global::MainCore.Notification.Message.QuestUpdated.HandleBehavior), lifetime));
		return services;
	}
}
