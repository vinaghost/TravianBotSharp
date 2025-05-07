using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Queries;

partial class GetVillageHasRallypointQuery
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Queries.GetVillageHasRallypointQuery.Query, global::MainCore.Entities.VillageId>
	{
		private readonly global::MainCore.Queries.GetVillageHasRallypointQuery.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Queries.GetVillageHasRallypointQuery.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(GetVillageHasRallypointQuery);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::MainCore.Entities.VillageId> HandleAsync(
			global::MainCore.Queries.GetVillageHasRallypointQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Queries.GetVillageHasRallypointQuery.Query, global::MainCore.Entities.VillageId>
	{
		private readonly global::MainCore.Infrasturecture.Persistence.AppDbContext _context;

		public HandleBehavior(
			global::MainCore.Infrasturecture.Persistence.AppDbContext context
		)
		{
			_context = context;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::MainCore.Entities.VillageId> HandleAsync(
			global::MainCore.Queries.GetVillageHasRallypointQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Queries.GetVillageHasRallypointQuery
				.HandleAsync(
					request
					, _context
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
		services.Add(new(typeof(global::MainCore.Queries.GetVillageHasRallypointQuery.Handler), typeof(global::MainCore.Queries.GetVillageHasRallypointQuery.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Queries.GetVillageHasRallypointQuery.Query, global::MainCore.Entities.VillageId>), typeof(global::MainCore.Queries.GetVillageHasRallypointQuery.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Queries.GetVillageHasRallypointQuery.HandleBehavior), typeof(global::MainCore.Queries.GetVillageHasRallypointQuery.HandleBehavior), lifetime));
		return services;
	}
}
