using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.Queries;

partial class VerifyAccessQuery
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Queries.VerifyAccessQuery.Query, bool>
	{
		private readonly global::MainCore.Commands.Queries.VerifyAccessQuery.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Commands.Queries.VerifyAccessQuery.Query, bool> _loggingBehavior;

		public Handler(
			global::MainCore.Commands.Queries.VerifyAccessQuery.HandleBehavior handleBehavior,
			global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Commands.Queries.VerifyAccessQuery.Query, bool> loggingBehavior
		)
		{
			var handlerType = typeof(VerifyAccessQuery);

			_handleBehavior = handleBehavior;

			_loggingBehavior = loggingBehavior;
			_loggingBehavior.HandlerType = handlerType;

			_loggingBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<bool> HandleAsync(
			global::MainCore.Commands.Queries.VerifyAccessQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _loggingBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.Queries.VerifyAccessQuery.Query, bool>
	{
		private readonly global::MainCore.Services.ILogService _logService;

		public HandleBehavior(
			global::MainCore.Services.ILogService logService
		)
		{
			_logService = logService;
		}

		public override async global::System.Threading.Tasks.ValueTask<bool> HandleAsync(
			global::MainCore.Commands.Queries.VerifyAccessQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Commands.Queries.VerifyAccessQuery
				.HandleAsync(
					request
					, _logService
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
		services.Add(new(typeof(global::MainCore.Commands.Queries.VerifyAccessQuery.Handler), typeof(global::MainCore.Commands.Queries.VerifyAccessQuery.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Queries.VerifyAccessQuery.Query, bool>), typeof(global::MainCore.Commands.Queries.VerifyAccessQuery.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.Queries.VerifyAccessQuery.HandleBehavior), typeof(global::MainCore.Commands.Queries.VerifyAccessQuery.HandleBehavior), lifetime));
		return services;
	}
}
