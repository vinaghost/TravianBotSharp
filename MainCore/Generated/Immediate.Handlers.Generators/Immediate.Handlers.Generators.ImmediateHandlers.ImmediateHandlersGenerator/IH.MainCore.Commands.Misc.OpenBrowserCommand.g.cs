using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.Misc;

partial class OpenBrowserCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Misc.OpenBrowserCommand.Command, global::System.ValueTuple>
	{
		private readonly global::MainCore.Commands.Misc.OpenBrowserCommand.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Commands.Misc.OpenBrowserCommand.Command, global::System.ValueTuple> _loggingBehavior;

		public Handler(
			global::MainCore.Commands.Misc.OpenBrowserCommand.HandleBehavior handleBehavior,
			global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Commands.Misc.OpenBrowserCommand.Command, global::System.ValueTuple> loggingBehavior
		)
		{
			var handlerType = typeof(OpenBrowserCommand);

			_handleBehavior = handleBehavior;

			_loggingBehavior = loggingBehavior;
			_loggingBehavior.HandlerType = handlerType;

			_loggingBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.Misc.OpenBrowserCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _loggingBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.Misc.OpenBrowserCommand.Command, global::System.ValueTuple>
	{
		private readonly global::MainCore.Services.IChromeManager _chromeManager;
		private readonly global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> _contextFactory;
		private readonly global::MainCore.Services.ILogService _logService;

		public HandleBehavior(
			global::MainCore.Services.IChromeManager chromeManager,
			global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> contextFactory,
			global::MainCore.Services.ILogService logService
		)
		{
			_chromeManager = chromeManager;
			_contextFactory = contextFactory;
			_logService = logService;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.Misc.OpenBrowserCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Commands.Misc.OpenBrowserCommand
				.HandleAsync(
					request
					, _chromeManager
					, _contextFactory
					, _logService
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
		services.Add(new(typeof(global::MainCore.Commands.Misc.OpenBrowserCommand.Handler), typeof(global::MainCore.Commands.Misc.OpenBrowserCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Misc.OpenBrowserCommand.Command, global::System.ValueTuple>), typeof(global::MainCore.Commands.Misc.OpenBrowserCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.Misc.OpenBrowserCommand.HandleBehavior), typeof(global::MainCore.Commands.Misc.OpenBrowserCommand.HandleBehavior), lifetime));
		return services;
	}
}
