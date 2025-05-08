using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Queries;

partial class GetTrainTroopBuildingQuery
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Queries.GetTrainTroopBuildingQuery.Query, global::System.Collections.Generic.List<global::MainCore.Enums.BuildingEnums>>
	{
		private readonly global::MainCore.Queries.GetTrainTroopBuildingQuery.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Queries.GetTrainTroopBuildingQuery.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(GetTrainTroopBuildingQuery);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.Collections.Generic.List<global::MainCore.Enums.BuildingEnums>> HandleAsync(
			global::MainCore.Queries.GetTrainTroopBuildingQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Queries.GetTrainTroopBuildingQuery.Query, global::System.Collections.Generic.List<global::MainCore.Enums.BuildingEnums>>
	{
		private readonly global::MainCore.Infrasturecture.Persistence.AppDbContext _context;

		public HandleBehavior(
			global::MainCore.Infrasturecture.Persistence.AppDbContext context
		)
		{
			_context = context;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.Collections.Generic.List<global::MainCore.Enums.BuildingEnums>> HandleAsync(
			global::MainCore.Queries.GetTrainTroopBuildingQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Queries.GetTrainTroopBuildingQuery
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
		services.Add(new(typeof(global::MainCore.Queries.GetTrainTroopBuildingQuery.Handler), typeof(global::MainCore.Queries.GetTrainTroopBuildingQuery.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Queries.GetTrainTroopBuildingQuery.Query, global::System.Collections.Generic.List<global::MainCore.Enums.BuildingEnums>>), typeof(global::MainCore.Queries.GetTrainTroopBuildingQuery.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Queries.GetTrainTroopBuildingQuery.HandleBehavior), typeof(global::MainCore.Queries.GetTrainTroopBuildingQuery.HandleBehavior), lifetime));
		return services;
	}
}
