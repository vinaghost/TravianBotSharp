using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.Misc;

partial class OpenBrowserCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Misc.OpenBrowserCommand.Command, global::System.ValueTuple>
	{
		private readonly global::MainCore.Commands.Misc.OpenBrowserCommand.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Commands.Misc.OpenBrowserCommand.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(OpenBrowserCommand);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.Misc.OpenBrowserCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.Misc.OpenBrowserCommand.Command, global::System.ValueTuple>
	{
		private readonly global::MainCore.Services.IChromeBrowser _browser;
		private readonly global::MainCore.Infrasturecture.Persistence.AppDbContext _context;
		private readonly global::Serilog.ILogger _logger;

		public HandleBehavior(
			global::MainCore.Services.IChromeBrowser browser,
			global::MainCore.Infrasturecture.Persistence.AppDbContext context,
			global::Serilog.ILogger logger
		)
		{
			_browser = browser;
			_context = context;
			_logger = logger;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.Misc.OpenBrowserCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Commands.Misc.OpenBrowserCommand
				.HandleAsync(
					request
					, _browser
					, _context
					, _logger
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
