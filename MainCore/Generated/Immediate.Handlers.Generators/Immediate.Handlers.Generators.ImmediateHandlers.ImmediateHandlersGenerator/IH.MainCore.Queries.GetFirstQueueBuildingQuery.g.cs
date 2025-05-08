using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Queries;

partial class GetFirstQueueBuildingQuery
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Queries.GetFirstQueueBuildingQuery.Query, global::MainCore.Entities.QueueBuilding>
	{
		private readonly global::MainCore.Queries.GetFirstQueueBuildingQuery.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Queries.GetFirstQueueBuildingQuery.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(GetFirstQueueBuildingQuery);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::MainCore.Entities.QueueBuilding> HandleAsync(
			global::MainCore.Queries.GetFirstQueueBuildingQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Queries.GetFirstQueueBuildingQuery.Query, global::MainCore.Entities.QueueBuilding>
	{
		private readonly global::MainCore.Infrasturecture.Persistence.AppDbContext _context;

		public HandleBehavior(
			global::MainCore.Infrasturecture.Persistence.AppDbContext context
		)
		{
			_context = context;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::MainCore.Entities.QueueBuilding> HandleAsync(
			global::MainCore.Queries.GetFirstQueueBuildingQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Queries.GetFirstQueueBuildingQuery
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
		services.Add(new(typeof(global::MainCore.Queries.GetFirstQueueBuildingQuery.Handler), typeof(global::MainCore.Queries.GetFirstQueueBuildingQuery.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Queries.GetFirstQueueBuildingQuery.Query, global::MainCore.Entities.QueueBuilding>), typeof(global::MainCore.Queries.GetFirstQueueBuildingQuery.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Queries.GetFirstQueueBuildingQuery.HandleBehavior), typeof(global::MainCore.Queries.GetFirstQueueBuildingQuery.HandleBehavior), lifetime));
		return services;
	}
}
