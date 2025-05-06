using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Tasks;

partial class LoginTask
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Tasks.LoginTask.Task, global::FluentResults.Result>
	{
		private readonly global::MainCore.Tasks.LoginTask.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Tasks.Behaviors.AccountTaskBehavior<global::MainCore.Tasks.LoginTask.Task, global::FluentResults.Result> _accountTaskBehavior;
		private readonly global::MainCore.Commands.Behaviors.CommandLoggingBehavior<global::MainCore.Tasks.LoginTask.Task, global::FluentResults.Result> _commandLoggingBehavior;

		public Handler(
			global::MainCore.Tasks.LoginTask.HandleBehavior handleBehavior,
			global::MainCore.Tasks.Behaviors.AccountTaskBehavior<global::MainCore.Tasks.LoginTask.Task, global::FluentResults.Result> accountTaskBehavior,
			global::MainCore.Commands.Behaviors.CommandLoggingBehavior<global::MainCore.Tasks.LoginTask.Task, global::FluentResults.Result> commandLoggingBehavior
		)
		{
			var handlerType = typeof(LoginTask);

			_handleBehavior = handleBehavior;

			_commandLoggingBehavior = commandLoggingBehavior;
			_commandLoggingBehavior.HandlerType = handlerType;

			_accountTaskBehavior = accountTaskBehavior;
			_accountTaskBehavior.HandlerType = handlerType;

			_accountTaskBehavior.SetInnerHandler(_handleBehavior);
			_commandLoggingBehavior.SetInnerHandler(_accountTaskBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Tasks.LoginTask.Task request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _commandLoggingBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Tasks.LoginTask.Task, global::FluentResults.Result>
	{
		private readonly global::MainCore.Commands.Features.LoginCommand.Handler _loginCommand;
		private readonly global::MainCore.Commands.Features.DisableContextualHelp.ToOptionsPageCommand.Handler _toOptionsPageCommand;
		private readonly global::MainCore.Commands.Features.DisableContextualHelp.DisableContextualHelpCommand.Handler _disableContextualHelpCommand;
		private readonly global::MainCore.Commands.Navigate.ToDorfCommand.Handler _toDorfCommand;

		public HandleBehavior(
			global::MainCore.Commands.Features.LoginCommand.Handler loginCommand,
			global::MainCore.Commands.Features.DisableContextualHelp.ToOptionsPageCommand.Handler toOptionsPageCommand,
			global::MainCore.Commands.Features.DisableContextualHelp.DisableContextualHelpCommand.Handler disableContextualHelpCommand,
			global::MainCore.Commands.Navigate.ToDorfCommand.Handler toDorfCommand
		)
		{
			_loginCommand = loginCommand;
			_toOptionsPageCommand = toOptionsPageCommand;
			_disableContextualHelpCommand = disableContextualHelpCommand;
			_toDorfCommand = toDorfCommand;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Tasks.LoginTask.Task request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Tasks.LoginTask
				.HandleAsync(
					request
					, _loginCommand
					, _toOptionsPageCommand
					, _disableContextualHelpCommand
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
		services.Add(new(typeof(global::MainCore.Tasks.LoginTask.Handler), typeof(global::MainCore.Tasks.LoginTask.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Tasks.LoginTask.Task, global::FluentResults.Result>), typeof(global::MainCore.Tasks.LoginTask.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Tasks.LoginTask.HandleBehavior), typeof(global::MainCore.Tasks.LoginTask.HandleBehavior), lifetime));
		return services;
	}
}
