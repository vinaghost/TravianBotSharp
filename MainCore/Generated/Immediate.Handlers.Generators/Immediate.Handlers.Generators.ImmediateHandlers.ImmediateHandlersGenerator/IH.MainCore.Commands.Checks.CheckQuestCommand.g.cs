using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.Checks;

partial class CheckQuestCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Checks.CheckQuestCommand.Command, global::System.ValueTuple>
	{
		private readonly global::MainCore.Commands.Checks.CheckQuestCommand.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Commands.Checks.CheckQuestCommand.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(CheckQuestCommand);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.Checks.CheckQuestCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.Checks.CheckQuestCommand.Command, global::System.ValueTuple>
	{
		private readonly global::MainCore.Services.IChromeBrowser _browser;
		private readonly global::MainCore.Notifications.Handlers.Trigger.ClaimQuestTaskTrigger.Handler _claimQuestTaskTrigger;

		public HandleBehavior(
			global::MainCore.Services.IChromeBrowser browser,
			global::MainCore.Notifications.Handlers.Trigger.ClaimQuestTaskTrigger.Handler claimQuestTaskTrigger
		)
		{
			_browser = browser;
			_claimQuestTaskTrigger = claimQuestTaskTrigger;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.Checks.CheckQuestCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Commands.Checks.CheckQuestCommand
				.HandleAsync(
					request
					, _browser
					, _claimQuestTaskTrigger
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
		services.Add(new(typeof(global::MainCore.Commands.Checks.CheckQuestCommand.Handler), typeof(global::MainCore.Commands.Checks.CheckQuestCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Checks.CheckQuestCommand.Command, global::System.ValueTuple>), typeof(global::MainCore.Commands.Checks.CheckQuestCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.Checks.CheckQuestCommand.HandleBehavior), typeof(global::MainCore.Commands.Checks.CheckQuestCommand.HandleBehavior), lifetime));
		return services;
	}
}
