using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Queries;

partial class GetVillagesQuery
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Queries.GetVillagesQuery.Query, global::System.Collections.Generic.List<global::MainCore.Entities.VillageId>>
	{
		private readonly global::MainCore.Queries.GetVillagesQuery.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Queries.GetVillagesQuery.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(GetVillagesQuery);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.Collections.Generic.List<global::MainCore.Entities.VillageId>> HandleAsync(
			global::MainCore.Queries.GetVillagesQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Queries.GetVillagesQuery.Query, global::System.Collections.Generic.List<global::MainCore.Entities.VillageId>>
	{
		private readonly global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> _contextFactory;

		public HandleBehavior(
			global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> contextFactory
		)
		{
			_contextFactory = contextFactory;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.Collections.Generic.List<global::MainCore.Entities.VillageId>> HandleAsync(
			global::MainCore.Queries.GetVillagesQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Queries.GetVillagesQuery
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
		services.Add(new(typeof(global::MainCore.Queries.GetVillagesQuery.Handler), typeof(global::MainCore.Queries.GetVillagesQuery.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Queries.GetVillagesQuery.Query, global::System.Collections.Generic.List<global::MainCore.Entities.VillageId>>), typeof(global::MainCore.Queries.GetVillagesQuery.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Queries.GetVillagesQuery.HandleBehavior), typeof(global::MainCore.Queries.GetVillagesQuery.HandleBehavior), lifetime));
		return services;
	}
}
