using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Tasks;

partial class SleepTask
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Tasks.SleepTask.Task, global::FluentResults.Result>
	{
		private readonly global::MainCore.Tasks.SleepTask.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Tasks.Behaviors.AccountTaskBehavior<global::MainCore.Tasks.SleepTask.Task, global::FluentResults.Result> _accountTaskBehavior;

		public Handler(
			global::MainCore.Tasks.SleepTask.HandleBehavior handleBehavior,
			global::MainCore.Tasks.Behaviors.AccountTaskBehavior<global::MainCore.Tasks.SleepTask.Task, global::FluentResults.Result> accountTaskBehavior
		)
		{
			var handlerType = typeof(SleepTask);

			_handleBehavior = handleBehavior;

			_accountTaskBehavior = accountTaskBehavior;
			_accountTaskBehavior.HandlerType = handlerType;

			_accountTaskBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Tasks.SleepTask.Task request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _accountTaskBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Tasks.SleepTask.Task, global::FluentResults.Result>
	{
		private readonly global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> _contextFactory;
		private readonly global::MainCore.Services.ITaskManager _taskManager;
		private readonly global::MainCore.Commands.Features.SleepCommand.Handler _sleepCommand;
		private readonly global::MainCore.Commands.Queries.GetAccessQuery.Handler _getAccessQuery;
		private readonly global::MainCore.Commands.Misc.OpenBrowserCommand.Handler _openBrowserCommand;
		private readonly global::MainCore.Commands.Navigate.ToDorfCommand.Handler _toDorfCommand;

		public HandleBehavior(
			global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> contextFactory,
			global::MainCore.Services.ITaskManager taskManager,
			global::MainCore.Commands.Features.SleepCommand.Handler sleepCommand,
			global::MainCore.Commands.Queries.GetAccessQuery.Handler getAccessQuery,
			global::MainCore.Commands.Misc.OpenBrowserCommand.Handler openBrowserCommand,
			global::MainCore.Commands.Navigate.ToDorfCommand.Handler toDorfCommand
		)
		{
			_contextFactory = contextFactory;
			_taskManager = taskManager;
			_sleepCommand = sleepCommand;
			_getAccessQuery = getAccessQuery;
			_openBrowserCommand = openBrowserCommand;
			_toDorfCommand = toDorfCommand;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Tasks.SleepTask.Task request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Tasks.SleepTask
				.HandleAsync(
					request
					, _contextFactory
					, _taskManager
					, _sleepCommand
					, _getAccessQuery
					, _openBrowserCommand
					, _toDorfCommand
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
		services.Add(new(typeof(global::MainCore.Tasks.SleepTask.Handler), typeof(global::MainCore.Tasks.SleepTask.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Tasks.SleepTask.Task, global::FluentResults.Result>), typeof(global::MainCore.Tasks.SleepTask.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Tasks.SleepTask.HandleBehavior), typeof(global::MainCore.Tasks.SleepTask.HandleBehavior), lifetime));
		return services;
	}
}
