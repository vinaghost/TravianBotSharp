using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Queries;

partial class GetValidAccessQuery
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Queries.GetValidAccessQuery.Query, global::FluentResults.Result<global::MainCore.DTO.AccessDto>>
	{
		private readonly global::MainCore.Queries.GetValidAccessQuery.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Queries.GetValidAccessQuery.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(GetValidAccessQuery);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result<global::MainCore.DTO.AccessDto>> HandleAsync(
			global::MainCore.Queries.GetValidAccessQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Queries.GetValidAccessQuery.Query, global::FluentResults.Result<global::MainCore.DTO.AccessDto>>
	{
		private readonly global::MainCore.Queries.GetAccessesQuery.Handler _getAccessesQuery;
		private readonly global::MainCore.Queries.VerifyAccessQuery.Handler _verifyAccessQuery;
		private readonly global::MainCore.Services.ISettingService _settingService;

		public HandleBehavior(
			global::MainCore.Queries.GetAccessesQuery.Handler getAccessesQuery,
			global::MainCore.Queries.VerifyAccessQuery.Handler verifyAccessQuery,
			global::MainCore.Services.ISettingService settingService
		)
		{
			_getAccessesQuery = getAccessesQuery;
			_verifyAccessQuery = verifyAccessQuery;
			_settingService = settingService;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result<global::MainCore.DTO.AccessDto>> HandleAsync(
			global::MainCore.Queries.GetValidAccessQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Queries.GetValidAccessQuery
				.HandleAsync(
					request
					, _getAccessesQuery
					, _verifyAccessQuery
					, _settingService
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
		services.Add(new(typeof(global::MainCore.Queries.GetValidAccessQuery.Handler), typeof(global::MainCore.Queries.GetValidAccessQuery.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Queries.GetValidAccessQuery.Query, global::FluentResults.Result<global::MainCore.DTO.AccessDto>>), typeof(global::MainCore.Queries.GetValidAccessQuery.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Queries.GetValidAccessQuery.HandleBehavior), typeof(global::MainCore.Queries.GetValidAccessQuery.HandleBehavior), lifetime));
		return services;
	}
}
