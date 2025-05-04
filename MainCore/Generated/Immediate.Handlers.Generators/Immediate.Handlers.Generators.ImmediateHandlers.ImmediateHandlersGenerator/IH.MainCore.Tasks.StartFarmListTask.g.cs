using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Tasks;

partial class StartFarmListTask
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Tasks.StartFarmListTask.Task, global::FluentResults.Result>
	{
		private readonly global::MainCore.Tasks.StartFarmListTask.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Tasks.Behaviors.AccountTaskBehavior<global::MainCore.Tasks.StartFarmListTask.Task, global::FluentResults.Result> _accountTaskBehavior;

		public Handler(
			global::MainCore.Tasks.StartFarmListTask.HandleBehavior handleBehavior,
			global::MainCore.Tasks.Behaviors.AccountTaskBehavior<global::MainCore.Tasks.StartFarmListTask.Task, global::FluentResults.Result> accountTaskBehavior
		)
		{
			var handlerType = typeof(StartFarmListTask);

			_handleBehavior = handleBehavior;

			_accountTaskBehavior = accountTaskBehavior;
			_accountTaskBehavior.HandlerType = handlerType;

			_accountTaskBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Tasks.StartFarmListTask.Task request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _accountTaskBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Tasks.StartFarmListTask.Task, global::FluentResults.Result>
	{
		private readonly global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> _contextFactory;
		private readonly global::MainCore.Services.ITaskManager _taskManager;
		private readonly global::MainCore.Commands.Features.StartFarmList.ToFarmListPageCommand.Handler _toFarmListPageCommand;
		private readonly global::MainCore.Commands.Features.StartFarmList.StartAllFarmListCommand.Handler _startAllFarmListCommand;
		private readonly global::MainCore.Commands.Features.StartFarmList.StartActiveFarmListCommand.Handler _startActiveFarmListCommand;

		public HandleBehavior(
			global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> contextFactory,
			global::MainCore.Services.ITaskManager taskManager,
			global::MainCore.Commands.Features.StartFarmList.ToFarmListPageCommand.Handler toFarmListPageCommand,
			global::MainCore.Commands.Features.StartFarmList.StartAllFarmListCommand.Handler startAllFarmListCommand,
			global::MainCore.Commands.Features.StartFarmList.StartActiveFarmListCommand.Handler startActiveFarmListCommand
		)
		{
			_contextFactory = contextFactory;
			_taskManager = taskManager;
			_toFarmListPageCommand = toFarmListPageCommand;
			_startAllFarmListCommand = startAllFarmListCommand;
			_startActiveFarmListCommand = startActiveFarmListCommand;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Tasks.StartFarmListTask.Task request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Tasks.StartFarmListTask
				.HandleAsync(
					request
					, _contextFactory
					, _taskManager
					, _toFarmListPageCommand
					, _startAllFarmListCommand
					, _startActiveFarmListCommand
					, cancellationToken
				)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public static IServiceCollection AddHandlers(
		IServiceCollection services,
		ServiceLifetime lifetime = ServiceLifetime.Scoped
	)
	{
		services.Add(new(typeof(global::MainCore.Tasks.StartFarmListTask.Handler), typeof(global::MainCore.Tasks.StartFarmListTask.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Tasks.StartFarmListTask.Task, global::FluentResults.Result>), typeof(global::MainCore.Tasks.StartFarmListTask.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Tasks.StartFarmListTask.HandleBehavior), typeof(global::MainCore.Tasks.StartFarmListTask.HandleBehavior), lifetime));
		return services;
	}
}
