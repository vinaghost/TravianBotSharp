using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Notification.Handlers.Trigger;

partial class BuildingUpdateTaskTrigger
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.ByAccountIdBase, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Handlers.Trigger.BuildingUpdateTaskTrigger.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Notification.ByAccountIdBase, global::System.ValueTuple> _loggingBehavior;

		public Handler(
			global::MainCore.Notification.Handlers.Trigger.BuildingUpdateTaskTrigger.HandleBehavior handleBehavior,
			global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Notification.ByAccountIdBase, global::System.ValueTuple> loggingBehavior
		)
		{
			var handlerType = typeof(BuildingUpdateTaskTrigger);

			_handleBehavior = handleBehavior;

			_loggingBehavior = loggingBehavior;
			_loggingBehavior.HandlerType = handlerType;

			_loggingBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.ByAccountIdBase request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _loggingBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Notification.ByAccountIdBase, global::System.ValueTuple>
	{
		private readonly global::MainCore.Services.ITaskManager _taskManager;
		private readonly global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> _contextFactory;
		private readonly global::MainCore.Commands.Queries.GetMissingBuildingVillagesQuery.Handler _getMissingBuildingVillageQuery;

		public HandleBehavior(
			global::MainCore.Services.ITaskManager taskManager,
			global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> contextFactory,
			global::MainCore.Commands.Queries.GetMissingBuildingVillagesQuery.Handler getMissingBuildingVillageQuery
		)
		{
			_taskManager = taskManager;
			_contextFactory = contextFactory;
			_getMissingBuildingVillageQuery = getMissingBuildingVillageQuery;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.ByAccountIdBase request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Notification.Handlers.Trigger.BuildingUpdateTaskTrigger
				.HandleAsync(
					request
					, _taskManager
					, _contextFactory
					, _getMissingBuildingVillageQuery
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
		services.Add(new(typeof(global::MainCore.Notification.Handlers.Trigger.BuildingUpdateTaskTrigger.Handler), typeof(global::MainCore.Notification.Handlers.Trigger.BuildingUpdateTaskTrigger.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.ByAccountIdBase, global::System.ValueTuple>), typeof(global::MainCore.Notification.Handlers.Trigger.BuildingUpdateTaskTrigger.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Notification.Handlers.Trigger.BuildingUpdateTaskTrigger.HandleBehavior), typeof(global::MainCore.Notification.Handlers.Trigger.BuildingUpdateTaskTrigger.HandleBehavior), lifetime));
		return services;
	}
}
