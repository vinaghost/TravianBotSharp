using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.Queries;

partial class GetAccessQuery
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Queries.GetAccessQuery.Query, global::FluentResults.Result<global::MainCore.DTO.AccessDto>>
	{
		private readonly global::MainCore.Commands.Queries.GetAccessQuery.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Commands.Queries.GetAccessQuery.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(GetAccessQuery);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result<global::MainCore.DTO.AccessDto>> HandleAsync(
			global::MainCore.Commands.Queries.GetAccessQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.Queries.GetAccessQuery.Query, global::FluentResults.Result<global::MainCore.DTO.AccessDto>>
	{
		private readonly global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> _contextFactory;
		private readonly global::MainCore.Commands.Queries.IGetSetting _getSetting;
		private readonly global::MainCore.Commands.Queries.VerifyAccessQuery.Handler _verifyAccessQuery;

		public HandleBehavior(
			global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> contextFactory,
			global::MainCore.Commands.Queries.IGetSetting getSetting,
			global::MainCore.Commands.Queries.VerifyAccessQuery.Handler verifyAccessQuery
		)
		{
			_contextFactory = contextFactory;
			_getSetting = getSetting;
			_verifyAccessQuery = verifyAccessQuery;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result<global::MainCore.DTO.AccessDto>> HandleAsync(
			global::MainCore.Commands.Queries.GetAccessQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Commands.Queries.GetAccessQuery
				.HandleAsync(
					request
					, _contextFactory
					, _getSetting
					, _verifyAccessQuery
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
		services.Add(new(typeof(global::MainCore.Commands.Queries.GetAccessQuery.Handler), typeof(global::MainCore.Commands.Queries.GetAccessQuery.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Queries.GetAccessQuery.Query, global::FluentResults.Result<global::MainCore.DTO.AccessDto>>), typeof(global::MainCore.Commands.Queries.GetAccessQuery.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.Queries.GetAccessQuery.HandleBehavior), typeof(global::MainCore.Commands.Queries.GetAccessQuery.HandleBehavior), lifetime));
		return services;
	}
}
