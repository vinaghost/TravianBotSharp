using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Queries;

partial class GetAccessesQuery
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Queries.GetAccessesQuery.Query, global::System.Collections.Generic.List<global::MainCore.DTO.AccessDto>>
	{
		private readonly global::MainCore.Queries.GetAccessesQuery.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Queries.GetAccessesQuery.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(GetAccessesQuery);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.Collections.Generic.List<global::MainCore.DTO.AccessDto>> HandleAsync(
			global::MainCore.Queries.GetAccessesQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Queries.GetAccessesQuery.Query, global::System.Collections.Generic.List<global::MainCore.DTO.AccessDto>>
	{
		private readonly global::MainCore.Infrasturecture.Persistence.AppDbContext _context;

		public HandleBehavior(
			global::MainCore.Infrasturecture.Persistence.AppDbContext context
		)
		{
			_context = context;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.Collections.Generic.List<global::MainCore.DTO.AccessDto>> HandleAsync(
			global::MainCore.Queries.GetAccessesQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Queries.GetAccessesQuery
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
		services.Add(new(typeof(global::MainCore.Queries.GetAccessesQuery.Handler), typeof(global::MainCore.Queries.GetAccessesQuery.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Queries.GetAccessesQuery.Query, global::System.Collections.Generic.List<global::MainCore.DTO.AccessDto>>), typeof(global::MainCore.Queries.GetAccessesQuery.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Queries.GetAccessesQuery.HandleBehavior), typeof(global::MainCore.Queries.GetAccessesQuery.HandleBehavior), lifetime));
		return services;
	}
}
