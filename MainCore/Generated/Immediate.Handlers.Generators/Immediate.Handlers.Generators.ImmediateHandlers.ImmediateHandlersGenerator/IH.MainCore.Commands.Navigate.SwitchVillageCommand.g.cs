using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.Navigate;

partial class SwitchVillageCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Navigate.SwitchVillageCommand.Command, global::FluentResults.Result>
	{
		private readonly global::MainCore.Commands.Navigate.SwitchVillageCommand.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Commands.Navigate.SwitchVillageCommand.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(SwitchVillageCommand);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Commands.Navigate.SwitchVillageCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.Navigate.SwitchVillageCommand.Command, global::FluentResults.Result>
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

		public override async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Commands.Navigate.SwitchVillageCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Commands.Navigate.SwitchVillageCommand
				.HandleAsync(
					request
					, _chromeManager
					, _questUpdated
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
		services.Add(new(typeof(global::MainCore.Commands.Navigate.SwitchVillageCommand.Handler), typeof(global::MainCore.Commands.Navigate.SwitchVillageCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Navigate.SwitchVillageCommand.Command, global::FluentResults.Result>), typeof(global::MainCore.Commands.Navigate.SwitchVillageCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.Navigate.SwitchVillageCommand.HandleBehavior), typeof(global::MainCore.Commands.Navigate.SwitchVillageCommand.HandleBehavior), lifetime));
		return services;
	}
}
