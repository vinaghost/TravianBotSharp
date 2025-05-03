using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.Checks;

partial class CheckAdventureCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Checks.CheckAdventureCommand.Command, global::System.ValueTuple>
	{
		private readonly global::MainCore.Commands.Checks.CheckAdventureCommand.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Commands.Checks.CheckAdventureCommand.Command, global::System.ValueTuple> _loggingBehavior;

		public Handler(
			global::MainCore.Commands.Checks.CheckAdventureCommand.HandleBehavior handleBehavior,
			global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Commands.Checks.CheckAdventureCommand.Command, global::System.ValueTuple> loggingBehavior
		)
		{
			var handlerType = typeof(CheckAdventureCommand);

			_handleBehavior = handleBehavior;

			_loggingBehavior = loggingBehavior;
			_loggingBehavior.HandlerType = handlerType;

			_loggingBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.Checks.CheckAdventureCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _loggingBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.Checks.CheckAdventureCommand.Command, global::System.ValueTuple>
	{
		private readonly global::MainCore.Services.IChromeManager _chromeManager;
		private readonly global::MainCore.Notification.Message.AdventureUpdated.Handler _adventureUpdated;

		public HandleBehavior(
			global::MainCore.Services.IChromeManager chromeManager,
			global::MainCore.Notification.Message.AdventureUpdated.Handler adventureUpdated
		)
		{
			_chromeManager = chromeManager;
			_adventureUpdated = adventureUpdated;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.Checks.CheckAdventureCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Commands.Checks.CheckAdventureCommand
				.HandleAsync(
					request
					, _chromeManager
					, _adventureUpdated
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
		services.Add(new(typeof(global::MainCore.Commands.Checks.CheckAdventureCommand.Handler), typeof(global::MainCore.Commands.Checks.CheckAdventureCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Checks.CheckAdventureCommand.Command, global::System.ValueTuple>), typeof(global::MainCore.Commands.Checks.CheckAdventureCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.Checks.CheckAdventureCommand.HandleBehavior), typeof(global::MainCore.Commands.Checks.CheckAdventureCommand.HandleBehavior), lifetime));
		return services;
	}
}
