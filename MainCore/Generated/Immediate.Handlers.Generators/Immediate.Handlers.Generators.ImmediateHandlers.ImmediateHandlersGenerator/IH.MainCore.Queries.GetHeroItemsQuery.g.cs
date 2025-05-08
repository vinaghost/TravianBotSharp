using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Queries;

partial class GetHeroItemsQuery
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Queries.GetHeroItemsQuery.Query, global::System.Collections.Generic.List<global::MainCore.Entities.HeroItem>>
	{
		private readonly global::MainCore.Queries.GetHeroItemsQuery.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Queries.GetHeroItemsQuery.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(GetHeroItemsQuery);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.Collections.Generic.List<global::MainCore.Entities.HeroItem>> HandleAsync(
			global::MainCore.Queries.GetHeroItemsQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Queries.GetHeroItemsQuery.Query, global::System.Collections.Generic.List<global::MainCore.Entities.HeroItem>>
	{
		private readonly global::MainCore.Infrasturecture.Persistence.AppDbContext _context;

		public HandleBehavior(
			global::MainCore.Infrasturecture.Persistence.AppDbContext context
		)
		{
			_context = context;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.Collections.Generic.List<global::MainCore.Entities.HeroItem>> HandleAsync(
			global::MainCore.Queries.GetHeroItemsQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Queries.GetHeroItemsQuery
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
		services.Add(new(typeof(global::MainCore.Queries.GetHeroItemsQuery.Handler), typeof(global::MainCore.Queries.GetHeroItemsQuery.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Queries.GetHeroItemsQuery.Query, global::System.Collections.Generic.List<global::MainCore.Entities.HeroItem>>), typeof(global::MainCore.Queries.GetHeroItemsQuery.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Queries.GetHeroItemsQuery.HandleBehavior), typeof(global::MainCore.Queries.GetHeroItemsQuery.HandleBehavior), lifetime));
		return services;
	}
}
