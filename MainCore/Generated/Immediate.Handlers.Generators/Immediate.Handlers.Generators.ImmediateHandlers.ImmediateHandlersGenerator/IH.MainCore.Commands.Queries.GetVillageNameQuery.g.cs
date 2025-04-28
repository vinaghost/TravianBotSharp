using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.Queries;

partial class GetVillageNameQuery
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Queries.GetVillageNameQuery.Query, string>
	{
		private readonly global::MainCore.Commands.Queries.GetVillageNameQuery.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Commands.Queries.GetVillageNameQuery.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(GetVillageNameQuery);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<string> HandleAsync(
			global::MainCore.Commands.Queries.GetVillageNameQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.Queries.GetVillageNameQuery.Query, string>
	{
		private readonly global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> _contextFactory;

		public HandleBehavior(
			global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> contextFactory
		)
		{
			_contextFactory = contextFactory;
		}

		public override async global::System.Threading.Tasks.ValueTask<string> HandleAsync(
			global::MainCore.Commands.Queries.GetVillageNameQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Commands.Queries.GetVillageNameQuery
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
		services.Add(new(typeof(global::MainCore.Commands.Queries.GetVillageNameQuery.Handler), typeof(global::MainCore.Commands.Queries.GetVillageNameQuery.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Queries.GetVillageNameQuery.Query, string>), typeof(global::MainCore.Commands.Queries.GetVillageNameQuery.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.Queries.GetVillageNameQuery.HandleBehavior), typeof(global::MainCore.Commands.Queries.GetVillageNameQuery.HandleBehavior), lifetime));
		return services;
	}
}
