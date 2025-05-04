using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.Navigate;

partial class ToBuildingCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Navigate.ToBuildingCommand.Command, global::FluentResults.Result>
	{
		private readonly global::MainCore.Commands.Navigate.ToBuildingCommand.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Commands.Navigate.ToBuildingCommand.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(ToBuildingCommand);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Commands.Navigate.ToBuildingCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.Navigate.ToBuildingCommand.Command, global::FluentResults.Result>
	{
		private readonly global::MainCore.Services.IChromeManager _chromeManager;

		public HandleBehavior(
			global::MainCore.Services.IChromeManager chromeManager
		)
		{
			_chromeManager = chromeManager;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Commands.Navigate.ToBuildingCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Commands.Navigate.ToBuildingCommand
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
		services.Add(new(typeof(global::MainCore.Commands.Navigate.ToBuildingCommand.Handler), typeof(global::MainCore.Commands.Navigate.ToBuildingCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Navigate.ToBuildingCommand.Command, global::FluentResults.Result>), typeof(global::MainCore.Commands.Navigate.ToBuildingCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.Navigate.ToBuildingCommand.HandleBehavior), typeof(global::MainCore.Commands.Navigate.ToBuildingCommand.HandleBehavior), lifetime));
		return services;
	}
}
