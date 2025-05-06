using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Tasks;

partial class StartAdventureTask
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Tasks.StartAdventureTask.Task, global::FluentResults.Result>
	{
		private readonly global::MainCore.Tasks.StartAdventureTask.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Tasks.Behaviors.AccountTaskBehavior<global::MainCore.Tasks.StartAdventureTask.Task, global::FluentResults.Result> _accountTaskBehavior;
		private readonly global::MainCore.Commands.Behaviors.CommandLoggingBehavior<global::MainCore.Tasks.StartAdventureTask.Task, global::FluentResults.Result> _commandLoggingBehavior;

		public Handler(
			global::MainCore.Tasks.StartAdventureTask.HandleBehavior handleBehavior,
			global::MainCore.Tasks.Behaviors.AccountTaskBehavior<global::MainCore.Tasks.StartAdventureTask.Task, global::FluentResults.Result> accountTaskBehavior,
			global::MainCore.Commands.Behaviors.CommandLoggingBehavior<global::MainCore.Tasks.StartAdventureTask.Task, global::FluentResults.Result> commandLoggingBehavior
		)
		{
			var handlerType = typeof(StartAdventureTask);

			_handleBehavior = handleBehavior;

			_commandLoggingBehavior = commandLoggingBehavior;
			_commandLoggingBehavior.HandlerType = handlerType;

			_accountTaskBehavior = accountTaskBehavior;
			_accountTaskBehavior.HandlerType = handlerType;

			_accountTaskBehavior.SetInnerHandler(_handleBehavior);
			_commandLoggingBehavior.SetInnerHandler(_accountTaskBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Tasks.StartAdventureTask.Task request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _commandLoggingBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Tasks.StartAdventureTask.Task, global::FluentResults.Result>
	{
		private readonly global::MainCore.Services.ITaskManager _taskManager;
		private readonly global::MainCore.Services.IChromeBrowser _browser;
		private readonly global::MainCore.Commands.Features.StartAdventure.ToAdventurePageCommand.Handler _toAdventurePageCommand;
		private readonly global::MainCore.Commands.Features.StartAdventure.ExploreAdventureCommand.Handler _exploreAdventureCommand;

		public HandleBehavior(
			global::MainCore.Services.ITaskManager taskManager,
			global::MainCore.Services.IChromeBrowser browser,
			global::MainCore.Commands.Features.StartAdventure.ToAdventurePageCommand.Handler toAdventurePageCommand,
			global::MainCore.Commands.Features.StartAdventure.ExploreAdventureCommand.Handler exploreAdventureCommand
		)
		{
			_taskManager = taskManager;
			_browser = browser;
			_toAdventurePageCommand = toAdventurePageCommand;
			_exploreAdventureCommand = exploreAdventureCommand;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Tasks.StartAdventureTask.Task request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Tasks.StartAdventureTask
				.HandleAsync(
					request
					, _taskManager
					, _browser
					, _toAdventurePageCommand
					, _exploreAdventureCommand
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
		services.Add(new(typeof(global::MainCore.Tasks.StartAdventureTask.Handler), typeof(global::MainCore.Tasks.StartAdventureTask.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Tasks.StartAdventureTask.Task, global::FluentResults.Result>), typeof(global::MainCore.Tasks.StartAdventureTask.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Tasks.StartAdventureTask.HandleBehavior), typeof(global::MainCore.Tasks.StartAdventureTask.HandleBehavior), lifetime));
		return services;
	}
}
