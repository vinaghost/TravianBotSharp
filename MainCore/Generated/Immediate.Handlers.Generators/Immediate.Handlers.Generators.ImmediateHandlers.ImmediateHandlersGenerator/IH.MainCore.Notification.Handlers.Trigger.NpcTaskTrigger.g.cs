using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Notification.Handlers.Trigger;

partial class NpcTaskTrigger
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.Base.IVillageNotification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Handlers.Trigger.NpcTaskTrigger.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Notification.Handlers.Trigger.NpcTaskTrigger.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(NpcTaskTrigger);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.Base.IVillageNotification request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Notification.Base.IVillageNotification, global::System.ValueTuple>
	{
		private readonly global::MainCore.Queries.GetVillageNameQuery.Handler _getVillageNameQuery;
		private readonly global::MainCore.Services.ITaskManager _taskManager;
		private readonly global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> _contextFactory;

		public HandleBehavior(
			global::MainCore.Queries.GetVillageNameQuery.Handler getVillageNameQuery,
			global::MainCore.Services.ITaskManager taskManager,
			global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> contextFactory
		)
		{
			_getVillageNameQuery = getVillageNameQuery;
			_taskManager = taskManager;
			_contextFactory = contextFactory;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.Base.IVillageNotification request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Notification.Handlers.Trigger.NpcTaskTrigger
				.HandleAsync(
					request
					, _getVillageNameQuery
					, _taskManager
					, _contextFactory
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
		services.Add(new(typeof(global::MainCore.Notification.Handlers.Trigger.NpcTaskTrigger.Handler), typeof(global::MainCore.Notification.Handlers.Trigger.NpcTaskTrigger.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.Base.IVillageNotification, global::System.ValueTuple>), typeof(global::MainCore.Notification.Handlers.Trigger.NpcTaskTrigger.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Notification.Handlers.Trigger.NpcTaskTrigger.HandleBehavior), typeof(global::MainCore.Notification.Handlers.Trigger.NpcTaskTrigger.HandleBehavior), lifetime));
		return services;
	}
}
