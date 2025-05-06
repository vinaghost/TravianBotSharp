using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.Features;

partial class SleepCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Features.SleepCommand.Command, global::FluentResults.Result>
	{
		private readonly global::MainCore.Commands.Features.SleepCommand.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Commands.Behaviors.CommandLoggingBehavior<global::MainCore.Commands.Features.SleepCommand.Command, global::FluentResults.Result> _commandLoggingBehavior;

		public Handler(
			global::MainCore.Commands.Features.SleepCommand.HandleBehavior handleBehavior,
			global::MainCore.Commands.Behaviors.CommandLoggingBehavior<global::MainCore.Commands.Features.SleepCommand.Command, global::FluentResults.Result> commandLoggingBehavior
		)
		{
			var handlerType = typeof(SleepCommand);

			_handleBehavior = handleBehavior;

			_commandLoggingBehavior = commandLoggingBehavior;
			_commandLoggingBehavior.HandlerType = handlerType;

			_commandLoggingBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Commands.Features.SleepCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _commandLoggingBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.Features.SleepCommand.Command, global::FluentResults.Result>
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

		public override async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Commands.Features.SleepCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Commands.Features.SleepCommand
				.HandleAsync(
					request
					, _browser
					, _context
					, _logger
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
		services.Add(new(typeof(global::MainCore.Commands.Features.SleepCommand.Handler), typeof(global::MainCore.Commands.Features.SleepCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Features.SleepCommand.Command, global::FluentResults.Result>), typeof(global::MainCore.Commands.Features.SleepCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.Features.SleepCommand.HandleBehavior), typeof(global::MainCore.Commands.Features.SleepCommand.HandleBehavior), lifetime));
		return services;
	}
}
