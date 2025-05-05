using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Queries;

partial class GetBuildingQuery
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Queries.GetBuildingQuery.Query, global::MainCore.DTO.BuildingDto>
	{
		private readonly global::MainCore.Queries.GetBuildingQuery.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Queries.GetBuildingQuery.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(GetBuildingQuery);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::MainCore.DTO.BuildingDto> HandleAsync(
			global::MainCore.Queries.GetBuildingQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Queries.GetBuildingQuery.Query, global::MainCore.DTO.BuildingDto>
	{
		private readonly global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> _contextFactory;

		public HandleBehavior(
			global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> contextFactory
		)
		{
			_contextFactory = contextFactory;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::MainCore.DTO.BuildingDto> HandleAsync(
			global::MainCore.Queries.GetBuildingQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Queries.GetBuildingQuery
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
		services.Add(new(typeof(global::MainCore.Queries.GetBuildingQuery.Handler), typeof(global::MainCore.Queries.GetBuildingQuery.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Queries.GetBuildingQuery.Query, global::MainCore.DTO.BuildingDto>), typeof(global::MainCore.Queries.GetBuildingQuery.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Queries.GetBuildingQuery.HandleBehavior), typeof(global::MainCore.Queries.GetBuildingQuery.HandleBehavior), lifetime));
		return services;
	}
}
