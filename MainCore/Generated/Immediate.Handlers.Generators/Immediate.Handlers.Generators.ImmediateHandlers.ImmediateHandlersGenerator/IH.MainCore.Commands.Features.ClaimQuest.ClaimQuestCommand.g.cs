using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.Features.ClaimQuest;

partial class ClaimQuestCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Features.ClaimQuest.ClaimQuestCommand.Command, global::System.ValueTuple>
	{
		private readonly global::MainCore.Commands.Features.ClaimQuest.ClaimQuestCommand.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Commands.Features.ClaimQuest.ClaimQuestCommand.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(ClaimQuestCommand);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.Features.ClaimQuest.ClaimQuestCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.Features.ClaimQuest.ClaimQuestCommand.Command, global::System.ValueTuple>
	{
		private readonly global::MainCore.Services.IChromeManager _chromeManager;
		private readonly global::MainCore.Notification.Message.StorageUpdated.Handler _storageUpdate;
		private readonly global::MainCore.Commands.Navigate.SwitchTabCommand.Handler _switchTabCommand;
		private readonly global::MainCore.Commands.Misc.DelayClickCommand.Handler _delayClickCommand;

		public HandleBehavior(
			global::MainCore.Services.IChromeManager chromeManager,
			global::MainCore.Notification.Message.StorageUpdated.Handler storageUpdate,
			global::MainCore.Commands.Navigate.SwitchTabCommand.Handler switchTabCommand,
			global::MainCore.Commands.Misc.DelayClickCommand.Handler delayClickCommand
		)
		{
			_chromeManager = chromeManager;
			_storageUpdate = storageUpdate;
			_switchTabCommand = switchTabCommand;
			_delayClickCommand = delayClickCommand;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.Features.ClaimQuest.ClaimQuestCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Commands.Features.ClaimQuest.ClaimQuestCommand
				.HandleAsync(
					request
					, _chromeManager
					, _storageUpdate
					, _switchTabCommand
					, _delayClickCommand
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
		services.Add(new(typeof(global::MainCore.Commands.Features.ClaimQuest.ClaimQuestCommand.Handler), typeof(global::MainCore.Commands.Features.ClaimQuest.ClaimQuestCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Features.ClaimQuest.ClaimQuestCommand.Command, global::System.ValueTuple>), typeof(global::MainCore.Commands.Features.ClaimQuest.ClaimQuestCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.Features.ClaimQuest.ClaimQuestCommand.HandleBehavior), typeof(global::MainCore.Commands.Features.ClaimQuest.ClaimQuestCommand.HandleBehavior), lifetime));
		return services;
	}
}
