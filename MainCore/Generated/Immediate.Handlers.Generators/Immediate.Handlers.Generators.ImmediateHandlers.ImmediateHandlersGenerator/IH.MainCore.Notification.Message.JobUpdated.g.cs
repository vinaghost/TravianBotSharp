using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Notification.Message;

partial class JobUpdated
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.Message.JobUpdated.Notification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Message.JobUpdated.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Notification.Message.JobUpdated.Notification, global::System.ValueTuple> _loggingBehavior;

		public Handler(
			global::MainCore.Notification.Message.JobUpdated.HandleBehavior handleBehavior,
			global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Notification.Message.JobUpdated.Notification, global::System.ValueTuple> loggingBehavior
		)
		{
			var handlerType = typeof(JobUpdated);

			_handleBehavior = handleBehavior;

			_loggingBehavior = loggingBehavior;
			_loggingBehavior.HandlerType = handlerType;

			_loggingBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.Message.JobUpdated.Notification request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _loggingBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Notification.Message.JobUpdated.Notification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Handlers.Trigger.UpgradeBuildingTaskTrigger.Handler _upgradeBuildingTaskTrigger;
		private readonly global::MainCore.Notification.Handlers.Refresh.JobListRefresh.Handler _jobListRefresh;

		public HandleBehavior(
			global::MainCore.Notification.Handlers.Trigger.UpgradeBuildingTaskTrigger.Handler upgradeBuildingTaskTrigger,
			global::MainCore.Notification.Handlers.Refresh.JobListRefresh.Handler jobListRefresh
		)
		{
			_upgradeBuildingTaskTrigger = upgradeBuildingTaskTrigger;
			_jobListRefresh = jobListRefresh;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.Message.JobUpdated.Notification request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Notification.Message.JobUpdated
				.HandleAsync(
					request
					, _upgradeBuildingTaskTrigger
					, _jobListRefresh
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
		services.Add(new(typeof(global::MainCore.Notification.Message.JobUpdated.Handler), typeof(global::MainCore.Notification.Message.JobUpdated.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.Message.JobUpdated.Notification, global::System.ValueTuple>), typeof(global::MainCore.Notification.Message.JobUpdated.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Notification.Message.JobUpdated.HandleBehavior), typeof(global::MainCore.Notification.Message.JobUpdated.HandleBehavior), lifetime));
		return services;
	}
}
