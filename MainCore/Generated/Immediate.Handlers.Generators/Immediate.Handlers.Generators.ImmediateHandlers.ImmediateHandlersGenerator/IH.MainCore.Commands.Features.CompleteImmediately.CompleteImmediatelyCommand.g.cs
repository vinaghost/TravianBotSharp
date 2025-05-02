using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.Features.CompleteImmediately;

partial class CompleteImmediatelyCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Features.CompleteImmediately.CompleteImmediatelyCommand.Command, global::FluentResults.Result>
	{
		private readonly global::MainCore.Commands.Features.CompleteImmediately.CompleteImmediatelyCommand.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Commands.Features.CompleteImmediately.CompleteImmediatelyCommand.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(CompleteImmediatelyCommand);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Commands.Features.CompleteImmediately.CompleteImmediatelyCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.Features.CompleteImmediately.CompleteImmediatelyCommand.Command, global::FluentResults.Result>
	{
		private readonly global::MainCore.Services.IChromeManager _chromeManager;
		private readonly global::MainCore.Notification.Message.CompleteImmediatelyMessage.Handler _completeImmediatelyMessage;

		public HandleBehavior(
			global::MainCore.Services.IChromeManager chromeManager,
			global::MainCore.Notification.Message.CompleteImmediatelyMessage.Handler completeImmediatelyMessage
		)
		{
			_chromeManager = chromeManager;
			_completeImmediatelyMessage = completeImmediatelyMessage;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Commands.Features.CompleteImmediately.CompleteImmediatelyCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Commands.Features.CompleteImmediately.CompleteImmediatelyCommand
				.HandleAsync(
					request
					, _chromeManager
					, _completeImmediatelyMessage
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
		services.Add(new(typeof(global::MainCore.Commands.Features.CompleteImmediately.CompleteImmediatelyCommand.Handler), typeof(global::MainCore.Commands.Features.CompleteImmediately.CompleteImmediatelyCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Features.CompleteImmediately.CompleteImmediatelyCommand.Command, global::FluentResults.Result>), typeof(global::MainCore.Commands.Features.CompleteImmediately.CompleteImmediatelyCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.Features.CompleteImmediately.CompleteImmediatelyCommand.HandleBehavior), typeof(global::MainCore.Commands.Features.CompleteImmediately.CompleteImmediatelyCommand.HandleBehavior), lifetime));
		return services;
	}
}
