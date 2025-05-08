using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Notifications.Message;

partial class JobUpdated
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notifications.Message.JobUpdated.Notification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notifications.Message.JobUpdated.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Notifications.Message.JobUpdated.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(JobUpdated);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notifications.Message.JobUpdated.Notification request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Notifications.Message.JobUpdated.Notification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notifications.Handlers.Trigger.UpgradeBuildingTaskTrigger.Handler _upgradeBuildingTaskTrigger;
		private readonly global::MainCore.Notifications.Handlers.Refresh.JobListRefresh.Handler _jobListRefresh;

		public HandleBehavior(
			global::MainCore.Notifications.Handlers.Trigger.UpgradeBuildingTaskTrigger.Handler upgradeBuildingTaskTrigger,
			global::MainCore.Notifications.Handlers.Refresh.JobListRefresh.Handler jobListRefresh
		)
		{
			_upgradeBuildingTaskTrigger = upgradeBuildingTaskTrigger;
			_jobListRefresh = jobListRefresh;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notifications.Message.JobUpdated.Notification request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Notifications.Message.JobUpdated
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
		services.Add(new(typeof(global::MainCore.Notifications.Message.JobUpdated.Handler), typeof(global::MainCore.Notifications.Message.JobUpdated.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notifications.Message.JobUpdated.Notification, global::System.ValueTuple>), typeof(global::MainCore.Notifications.Message.JobUpdated.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Notifications.Message.JobUpdated.HandleBehavior), typeof(global::MainCore.Notifications.Message.JobUpdated.HandleBehavior), lifetime));
		return services;
	}
}
