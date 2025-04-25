using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.Queries;

partial class IsResourceEnough
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Queries.IsResourceEnough.Query, global::FluentResults.Result>
	{
		private readonly global::MainCore.Commands.Queries.IsResourceEnough.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Commands.Queries.IsResourceEnough.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(IsResourceEnough);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Commands.Queries.IsResourceEnough.Query request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.Queries.IsResourceEnough.Query, global::FluentResults.Result>
	{
		private readonly global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> _contextFactory;

		public HandleBehavior(
			global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> contextFactory
		)
		{
			_contextFactory = contextFactory;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Commands.Queries.IsResourceEnough.Query request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Commands.Queries.IsResourceEnough
				.HandleAsync(
					request
					, _contextFactory
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
		services.Add(new(typeof(global::MainCore.Commands.Queries.IsResourceEnough.Handler), typeof(global::MainCore.Commands.Queries.IsResourceEnough.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Queries.IsResourceEnough.Query, global::FluentResults.Result>), typeof(global::MainCore.Commands.Queries.IsResourceEnough.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.Queries.IsResourceEnough.HandleBehavior), typeof(global::MainCore.Commands.Queries.IsResourceEnough.HandleBehavior), lifetime));
		return services;
	}
}
