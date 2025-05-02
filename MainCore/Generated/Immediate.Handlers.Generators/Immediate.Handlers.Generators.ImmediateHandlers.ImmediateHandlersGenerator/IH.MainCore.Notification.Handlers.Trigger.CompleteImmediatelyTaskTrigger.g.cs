using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Notification.Handlers.Trigger;

partial class CompleteImmediatelyTaskTrigger
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.ByAccountVillageIdBase, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Handlers.Trigger.CompleteImmediatelyTaskTrigger.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Notification.Handlers.Trigger.CompleteImmediatelyTaskTrigger.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(CompleteImmediatelyTaskTrigger);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.ByAccountVillageIdBase request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Notification.ByAccountVillageIdBase, global::System.ValueTuple>
	{
		private readonly global::MainCore.Services.ITaskManager _taskManager;
		private readonly global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> _contextFactory;
		private readonly IGetSetting _getSetting;

		public HandleBehavior(
			global::MainCore.Services.ITaskManager taskManager,
			global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> contextFactory,
			IGetSetting getSetting
		)
		{
			_taskManager = taskManager;
			_contextFactory = contextFactory;
			_getSetting = getSetting;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.ByAccountVillageIdBase request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Notification.Handlers.Trigger.CompleteImmediatelyTaskTrigger
				.HandleAsync(
					request
					, _taskManager
					, _contextFactory
					, _getSetting
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
		services.Add(new(typeof(global::MainCore.Notification.Handlers.Trigger.CompleteImmediatelyTaskTrigger.Handler), typeof(global::MainCore.Notification.Handlers.Trigger.CompleteImmediatelyTaskTrigger.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.ByAccountVillageIdBase, global::System.ValueTuple>), typeof(global::MainCore.Notification.Handlers.Trigger.CompleteImmediatelyTaskTrigger.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Notification.Handlers.Trigger.CompleteImmediatelyTaskTrigger.HandleBehavior), typeof(global::MainCore.Notification.Handlers.Trigger.CompleteImmediatelyTaskTrigger.HandleBehavior), lifetime));
		return services;
	}
}
