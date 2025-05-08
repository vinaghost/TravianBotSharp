using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Tasks;

partial class SleepTask
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Tasks.SleepTask.Task, global::FluentResults.Result>
	{
		private readonly global::MainCore.Tasks.SleepTask.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Tasks.Behaviors.AccountTaskBehavior<global::MainCore.Tasks.SleepTask.Task, global::FluentResults.Result> _accountTaskBehavior;
		private readonly global::MainCore.Commands.Behaviors.CommandLoggingBehavior<global::MainCore.Tasks.SleepTask.Task, global::FluentResults.Result> _commandLoggingBehavior;

		public Handler(
			global::MainCore.Tasks.SleepTask.HandleBehavior handleBehavior,
			global::MainCore.Tasks.Behaviors.AccountTaskBehavior<global::MainCore.Tasks.SleepTask.Task, global::FluentResults.Result> accountTaskBehavior,
			global::MainCore.Commands.Behaviors.CommandLoggingBehavior<global::MainCore.Tasks.SleepTask.Task, global::FluentResults.Result> commandLoggingBehavior
		)
		{
			var handlerType = typeof(SleepTask);

			_handleBehavior = handleBehavior;

			_commandLoggingBehavior = commandLoggingBehavior;
			_commandLoggingBehavior.HandlerType = handlerType;

			_accountTaskBehavior = accountTaskBehavior;
			_accountTaskBehavior.HandlerType = handlerType;

			_accountTaskBehavior.SetInnerHandler(_handleBehavior);
			_commandLoggingBehavior.SetInnerHandler(_accountTaskBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Tasks.SleepTask.Task request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _commandLoggingBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Tasks.SleepTask.Task, global::FluentResults.Result>
	{
		private readonly global::MainCore.Commands.Features.SleepCommand.Handler _sleepCommand;
		private readonly global::MainCore.Queries.GetValidAccessQuery.Handler _getAccessQuery;
		private readonly global::MainCore.Commands.Misc.OpenBrowserCommand.Handler _openBrowserCommand;
		private readonly global::MainCore.Commands.Navigate.ToDorfCommand.Handler _toDorfCommand;
		private readonly global::MainCore.Commands.NextExecute.NextExecuteSleepTaskCommand.Handler _nextExecuteSleepTaskCommand;

		public HandleBehavior(
			global::MainCore.Commands.Features.SleepCommand.Handler sleepCommand,
			global::MainCore.Queries.GetValidAccessQuery.Handler getAccessQuery,
			global::MainCore.Commands.Misc.OpenBrowserCommand.Handler openBrowserCommand,
			global::MainCore.Commands.Navigate.ToDorfCommand.Handler toDorfCommand,
			global::MainCore.Commands.NextExecute.NextExecuteSleepTaskCommand.Handler nextExecuteSleepTaskCommand
		)
		{
			_sleepCommand = sleepCommand;
			_getAccessQuery = getAccessQuery;
			_openBrowserCommand = openBrowserCommand;
			_toDorfCommand = toDorfCommand;
			_nextExecuteSleepTaskCommand = nextExecuteSleepTaskCommand;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Tasks.SleepTask.Task request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Tasks.SleepTask
				.HandleAsync(
					request
					, _sleepCommand
					, _getAccessQuery
					, _openBrowserCommand
					, _toDorfCommand
					, _nextExecuteSleepTaskCommand
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
