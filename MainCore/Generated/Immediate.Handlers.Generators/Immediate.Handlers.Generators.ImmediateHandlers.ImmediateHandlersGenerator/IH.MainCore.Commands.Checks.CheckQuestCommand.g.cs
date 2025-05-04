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
		private readonly global::MainCore.Services.IChromeManager _chromeManager;
		private readonly global::MainCore.Notification.Message.QuestUpdated.Handler _questUpdated;

		public HandleBehavior(
			global::MainCore.Services.IChromeManager chromeManager,
			global::MainCore.Notification.Message.QuestUpdated.Handler questUpdated
		)
		{
			_chromeManager = chromeManager;
			_questUpdated = questUpdated;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.Checks.CheckQuestCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Commands.Checks.CheckQuestCommand
				.HandleAsync(
					request
					, _chromeManager
					, _questUpdated
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
