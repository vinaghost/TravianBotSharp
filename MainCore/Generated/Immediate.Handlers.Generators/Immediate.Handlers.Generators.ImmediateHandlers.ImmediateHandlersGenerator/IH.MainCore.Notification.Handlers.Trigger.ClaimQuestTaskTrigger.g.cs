using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Notification.Handlers.Trigger;

partial class ClaimQuestTaskTrigger
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.ByAccountVillageIdBase, global::System.ValueTuple>
	{
		private readonly global::MainCore.Notification.Handlers.Trigger.ClaimQuestTaskTrigger.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Notification.ByAccountVillageIdBase, global::System.ValueTuple> _loggingBehavior;

		public Handler(
			global::MainCore.Notification.Handlers.Trigger.ClaimQuestTaskTrigger.HandleBehavior handleBehavior,
			global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Notification.ByAccountVillageIdBase, global::System.ValueTuple> loggingBehavior
		)
		{
			var handlerType = typeof(ClaimQuestTaskTrigger);

			_handleBehavior = handleBehavior;

			_loggingBehavior = loggingBehavior;
			_loggingBehavior.HandlerType = handlerType;

			_loggingBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.ByAccountVillageIdBase request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _loggingBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Notification.ByAccountVillageIdBase, global::System.ValueTuple>
	{
		private readonly global::MainCore.Services.ITaskManager _taskManager;
		private readonly global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> _contextFactory;

		public HandleBehavior(
			global::MainCore.Services.ITaskManager taskManager,
			global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> contextFactory
		)
		{
			_taskManager = taskManager;
			_contextFactory = contextFactory;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Notification.ByAccountVillageIdBase request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Notification.Handlers.Trigger.ClaimQuestTaskTrigger
				.HandleAsync(
					request
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
		services.Add(new(typeof(global::MainCore.Notification.Handlers.Trigger.ClaimQuestTaskTrigger.Handler), typeof(global::MainCore.Notification.Handlers.Trigger.ClaimQuestTaskTrigger.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Notification.ByAccountVillageIdBase, global::System.ValueTuple>), typeof(global::MainCore.Notification.Handlers.Trigger.ClaimQuestTaskTrigger.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Notification.Handlers.Trigger.ClaimQuestTaskTrigger.HandleBehavior), typeof(global::MainCore.Notification.Handlers.Trigger.ClaimQuestTaskTrigger.HandleBehavior), lifetime));
		return services;
	}
}
