using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.Queries;

partial class GetMissingBuildingVillagesQuery
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Queries.GetMissingBuildingVillagesQuery.Query, global::System.Collections.Generic.List<global::MainCore.Entities.VillageId>>
	{
		private readonly global::MainCore.Commands.Queries.GetMissingBuildingVillagesQuery.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Commands.Queries.GetMissingBuildingVillagesQuery.Query, global::System.Collections.Generic.List<global::MainCore.Entities.VillageId>> _loggingBehavior;

		public Handler(
			global::MainCore.Commands.Queries.GetMissingBuildingVillagesQuery.HandleBehavior handleBehavior,
			global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Commands.Queries.GetMissingBuildingVillagesQuery.Query, global::System.Collections.Generic.List<global::MainCore.Entities.VillageId>> loggingBehavior
		)
		{
			var handlerType = typeof(GetMissingBuildingVillagesQuery);

			_handleBehavior = handleBehavior;

			_loggingBehavior = loggingBehavior;
			_loggingBehavior.HandlerType = handlerType;

			_loggingBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::System.Collections.Generic.List<global::MainCore.Entities.VillageId>> HandleAsync(
			global::MainCore.Commands.Queries.GetMissingBuildingVillagesQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _loggingBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.Queries.GetMissingBuildingVillagesQuery.Query, global::System.Collections.Generic.List<global::MainCore.Entities.VillageId>>
	{
		private readonly global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> _contextFactory;

		public HandleBehavior(
			global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> contextFactory
		)
		{
			_contextFactory = contextFactory;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.Collections.Generic.List<global::MainCore.Entities.VillageId>> HandleAsync(
			global::MainCore.Commands.Queries.GetMissingBuildingVillagesQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Commands.Queries.GetMissingBuildingVillagesQuery
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
		services.Add(new(typeof(global::MainCore.Commands.Queries.GetMissingBuildingVillagesQuery.Handler), typeof(global::MainCore.Commands.Queries.GetMissingBuildingVillagesQuery.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Queries.GetMissingBuildingVillagesQuery.Query, global::System.Collections.Generic.List<global::MainCore.Entities.VillageId>>), typeof(global::MainCore.Commands.Queries.GetMissingBuildingVillagesQuery.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.Queries.GetMissingBuildingVillagesQuery.HandleBehavior), typeof(global::MainCore.Commands.Queries.GetMissingBuildingVillagesQuery.HandleBehavior), lifetime));
		return services;
	}
}
