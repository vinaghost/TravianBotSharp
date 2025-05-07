using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.Features.UseHeroItem;

partial class UseHeroResourceCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Features.UseHeroItem.UseHeroResourceCommand.Command, global::FluentResults.Result>
	{
		private readonly global::MainCore.Commands.Features.UseHeroItem.UseHeroResourceCommand.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Commands.Behaviors.CommandLoggingBehavior<global::MainCore.Commands.Features.UseHeroItem.UseHeroResourceCommand.Command, global::FluentResults.Result> _commandLoggingBehavior;

		public Handler(
			global::MainCore.Commands.Features.UseHeroItem.UseHeroResourceCommand.HandleBehavior handleBehavior,
			global::MainCore.Commands.Behaviors.CommandLoggingBehavior<global::MainCore.Commands.Features.UseHeroItem.UseHeroResourceCommand.Command, global::FluentResults.Result> commandLoggingBehavior
		)
		{
			var handlerType = typeof(UseHeroResourceCommand);

			_handleBehavior = handleBehavior;

			_commandLoggingBehavior = commandLoggingBehavior;
			_commandLoggingBehavior.HandlerType = handlerType;

			_commandLoggingBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Commands.Features.UseHeroItem.UseHeroResourceCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _commandLoggingBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.Features.UseHeroItem.UseHeroResourceCommand.Command, global::FluentResults.Result>
	{
		private readonly global::MainCore.Services.IChromeBrowser _browser;
		private readonly global::MainCore.Commands.Misc.DelayClickCommand.Handler _delayClickCommand;
		private readonly global::MainCore.Queries.GetHeroItemsQuery.Handler _getHeroItemsQuery;

		public HandleBehavior(
			global::MainCore.Services.IChromeBrowser browser,
			global::MainCore.Commands.Misc.DelayClickCommand.Handler delayClickCommand,
			global::MainCore.Queries.GetHeroItemsQuery.Handler getHeroItemsQuery
		)
		{
			_browser = browser;
			_delayClickCommand = delayClickCommand;
			_getHeroItemsQuery = getHeroItemsQuery;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Commands.Features.UseHeroItem.UseHeroResourceCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Commands.Features.UseHeroItem.UseHeroResourceCommand
				.HandleAsync(
					request
					, _browser
					, _delayClickCommand
					, _getHeroItemsQuery
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
		services.Add(new(typeof(global::MainCore.Commands.Features.UseHeroItem.UseHeroResourceCommand.Handler), typeof(global::MainCore.Commands.Features.UseHeroItem.UseHeroResourceCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Features.UseHeroItem.UseHeroResourceCommand.Command, global::FluentResults.Result>), typeof(global::MainCore.Commands.Features.UseHeroItem.UseHeroResourceCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.Features.UseHeroItem.UseHeroResourceCommand.HandleBehavior), typeof(global::MainCore.Commands.Features.UseHeroItem.UseHeroResourceCommand.HandleBehavior), lifetime));
		return services;
	}
}
