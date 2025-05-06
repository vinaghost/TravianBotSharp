using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Queries;

partial class VerifyAccessQuery
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Queries.VerifyAccessQuery.Query, bool>
	{
		private readonly global::MainCore.Queries.VerifyAccessQuery.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Queries.VerifyAccessQuery.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(VerifyAccessQuery);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<bool> HandleAsync(
			global::MainCore.Queries.VerifyAccessQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Queries.VerifyAccessQuery.Query, bool>
	{
		private readonly global::Serilog.ILogger _logger;

		public HandleBehavior(
			global::Serilog.ILogger logger
		)
		{
			_logger = logger;
		}

		public override async global::System.Threading.Tasks.ValueTask<bool> HandleAsync(
			global::MainCore.Queries.VerifyAccessQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Queries.VerifyAccessQuery
				.HandleAsync(
					request
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
		services.Add(new(typeof(global::MainCore.Queries.VerifyAccessQuery.Handler), typeof(global::MainCore.Queries.VerifyAccessQuery.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Queries.VerifyAccessQuery.Query, bool>), typeof(global::MainCore.Queries.VerifyAccessQuery.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Queries.VerifyAccessQuery.HandleBehavior), typeof(global::MainCore.Queries.VerifyAccessQuery.HandleBehavior), lifetime));
		return services;
	}
}
