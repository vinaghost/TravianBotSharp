using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.Features.DisableContextualHelp;

partial class DisableContextualHelpCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Features.DisableContextualHelp.DisableContextualHelpCommand.Command, global::FluentResults.Result>
	{
		private readonly global::MainCore.Commands.Features.DisableContextualHelp.DisableContextualHelpCommand.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Commands.Features.DisableContextualHelp.DisableContextualHelpCommand.Command, global::FluentResults.Result> _loggingBehavior;

		public Handler(
			global::MainCore.Commands.Features.DisableContextualHelp.DisableContextualHelpCommand.HandleBehavior handleBehavior,
			global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Commands.Features.DisableContextualHelp.DisableContextualHelpCommand.Command, global::FluentResults.Result> loggingBehavior
		)
		{
			var handlerType = typeof(DisableContextualHelpCommand);

			_handleBehavior = handleBehavior;

			_loggingBehavior = loggingBehavior;
			_loggingBehavior.HandlerType = handlerType;

			_loggingBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Commands.Features.DisableContextualHelp.DisableContextualHelpCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _loggingBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.Features.DisableContextualHelp.DisableContextualHelpCommand.Command, global::FluentResults.Result>
	{
		private readonly global::MainCore.Services.IChromeManager _chromeManager;

		public HandleBehavior(
			global::MainCore.Services.IChromeManager chromeManager
		)
		{
			_chromeManager = chromeManager;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Commands.Features.DisableContextualHelp.DisableContextualHelpCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Commands.Features.DisableContextualHelp.DisableContextualHelpCommand
				.HandleAsync(
					request
					, _chromeManager
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
		services.Add(new(typeof(global::MainCore.Commands.Features.DisableContextualHelp.DisableContextualHelpCommand.Handler), typeof(global::MainCore.Commands.Features.DisableContextualHelp.DisableContextualHelpCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Features.DisableContextualHelp.DisableContextualHelpCommand.Command, global::FluentResults.Result>), typeof(global::MainCore.Commands.Features.DisableContextualHelp.DisableContextualHelpCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.Features.DisableContextualHelp.DisableContextualHelpCommand.HandleBehavior), typeof(global::MainCore.Commands.Features.DisableContextualHelp.DisableContextualHelpCommand.HandleBehavior), lifetime));
		return services;
	}
}
