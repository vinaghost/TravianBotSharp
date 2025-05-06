using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.Features.ClaimQuest;

partial class ToQuestPageCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Features.ClaimQuest.ToQuestPageCommand.Command, global::FluentResults.Result>
	{
		private readonly global::MainCore.Commands.Features.ClaimQuest.ToQuestPageCommand.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Commands.Behaviors.CommandLoggingBehavior<global::MainCore.Commands.Features.ClaimQuest.ToQuestPageCommand.Command, global::FluentResults.Result> _commandLoggingBehavior;

		public Handler(
			global::MainCore.Commands.Features.ClaimQuest.ToQuestPageCommand.HandleBehavior handleBehavior,
			global::MainCore.Commands.Behaviors.CommandLoggingBehavior<global::MainCore.Commands.Features.ClaimQuest.ToQuestPageCommand.Command, global::FluentResults.Result> commandLoggingBehavior
		)
		{
			var handlerType = typeof(ToQuestPageCommand);

			_handleBehavior = handleBehavior;

			_commandLoggingBehavior = commandLoggingBehavior;
			_commandLoggingBehavior.HandlerType = handlerType;

			_commandLoggingBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Commands.Features.ClaimQuest.ToQuestPageCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _commandLoggingBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.Features.ClaimQuest.ToQuestPageCommand.Command, global::FluentResults.Result>
	{
		private readonly global::MainCore.Services.IChromeBrowser _browser;

		public HandleBehavior(
			global::MainCore.Services.IChromeBrowser browser
		)
		{
			_browser = browser;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Commands.Features.ClaimQuest.ToQuestPageCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Commands.Features.ClaimQuest.ToQuestPageCommand
				.HandleAsync(
					request
					, _browser
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
		services.Add(new(typeof(global::MainCore.Commands.Features.ClaimQuest.ToQuestPageCommand.Handler), typeof(global::MainCore.Commands.Features.ClaimQuest.ToQuestPageCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Features.ClaimQuest.ToQuestPageCommand.Command, global::FluentResults.Result>), typeof(global::MainCore.Commands.Features.ClaimQuest.ToQuestPageCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.Features.ClaimQuest.ToQuestPageCommand.HandleBehavior), typeof(global::MainCore.Commands.Features.ClaimQuest.ToQuestPageCommand.HandleBehavior), lifetime));
		return services;
	}
}
