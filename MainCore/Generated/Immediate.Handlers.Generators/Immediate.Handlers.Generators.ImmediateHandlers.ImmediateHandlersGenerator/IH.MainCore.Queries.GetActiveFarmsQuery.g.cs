using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Queries;

partial class GetActiveFarmsQuery
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Queries.GetActiveFarmsQuery.Query, global::System.Collections.Generic.List<global::MainCore.Entities.FarmId>>
	{
		private readonly global::MainCore.Queries.GetActiveFarmsQuery.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Queries.GetActiveFarmsQuery.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(GetActiveFarmsQuery);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.Collections.Generic.List<global::MainCore.Entities.FarmId>> HandleAsync(
			global::MainCore.Queries.GetActiveFarmsQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Queries.GetActiveFarmsQuery.Query, global::System.Collections.Generic.List<global::MainCore.Entities.FarmId>>
	{
		private readonly global::MainCore.Infrasturecture.Persistence.AppDbContext _context;

		public HandleBehavior(
			global::MainCore.Infrasturecture.Persistence.AppDbContext context
		)
		{
			_context = context;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.Collections.Generic.List<global::MainCore.Entities.FarmId>> HandleAsync(
			global::MainCore.Queries.GetActiveFarmsQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Queries.GetActiveFarmsQuery
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
		services.Add(new(typeof(global::MainCore.Queries.GetActiveFarmsQuery.Handler), typeof(global::MainCore.Queries.GetActiveFarmsQuery.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Queries.GetActiveFarmsQuery.Query, global::System.Collections.Generic.List<global::MainCore.Entities.FarmId>>), typeof(global::MainCore.Queries.GetActiveFarmsQuery.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Queries.GetActiveFarmsQuery.HandleBehavior), typeof(global::MainCore.Queries.GetActiveFarmsQuery.HandleBehavior), lifetime));
		return services;
	}
}
