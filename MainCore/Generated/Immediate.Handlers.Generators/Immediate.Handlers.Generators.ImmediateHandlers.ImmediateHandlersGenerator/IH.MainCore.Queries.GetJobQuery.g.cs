using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Queries;

partial class GetJobQuery
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Queries.GetJobQuery.Query, global::FluentResults.Result<global::MainCore.DTO.JobDto>>
	{
		private readonly global::MainCore.Queries.GetJobQuery.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Queries.GetJobQuery.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(GetJobQuery);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result<global::MainCore.DTO.JobDto>> HandleAsync(
			global::MainCore.Queries.GetJobQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Queries.GetJobQuery.Query, global::FluentResults.Result<global::MainCore.DTO.JobDto>>
	{
		private readonly global::MainCore.Infrasturecture.Persistence.AppDbContext _context;

		public HandleBehavior(
			global::MainCore.Infrasturecture.Persistence.AppDbContext context
		)
		{
			_context = context;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result<global::MainCore.DTO.JobDto>> HandleAsync(
			global::MainCore.Queries.GetJobQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Queries.GetJobQuery
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
		services.Add(new(typeof(global::MainCore.Queries.GetJobQuery.Handler), typeof(global::MainCore.Queries.GetJobQuery.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Queries.GetJobQuery.Query, global::FluentResults.Result<global::MainCore.DTO.JobDto>>), typeof(global::MainCore.Queries.GetJobQuery.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Queries.GetJobQuery.HandleBehavior), typeof(global::MainCore.Queries.GetJobQuery.HandleBehavior), lifetime));
		return services;
	}
}
