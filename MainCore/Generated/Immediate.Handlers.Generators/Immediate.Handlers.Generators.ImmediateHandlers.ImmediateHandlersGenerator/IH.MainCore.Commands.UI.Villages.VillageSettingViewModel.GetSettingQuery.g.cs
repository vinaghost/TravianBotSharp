using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.UI.Villages.VillageSettingViewModel;

partial class GetSettingQuery
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.Villages.VillageSettingViewModel.GetSettingQuery.Query, global::System.Collections.Generic.Dictionary<global::MainCore.Enums.VillageSettingEnums, int>>
	{
		private readonly global::MainCore.Commands.UI.Villages.VillageSettingViewModel.GetSettingQuery.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Commands.UI.Villages.VillageSettingViewModel.GetSettingQuery.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(GetSettingQuery);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.Collections.Generic.Dictionary<global::MainCore.Enums.VillageSettingEnums, int>> HandleAsync(
			global::MainCore.Commands.UI.Villages.VillageSettingViewModel.GetSettingQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.UI.Villages.VillageSettingViewModel.GetSettingQuery.Query, global::System.Collections.Generic.Dictionary<global::MainCore.Enums.VillageSettingEnums, int>>
	{
		private readonly global::MainCore.Infrasturecture.Persistence.AppDbContext _context;

		public HandleBehavior(
			global::MainCore.Infrasturecture.Persistence.AppDbContext context
		)
		{
			_context = context;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.Collections.Generic.Dictionary<global::MainCore.Enums.VillageSettingEnums, int>> HandleAsync(
			global::MainCore.Commands.UI.Villages.VillageSettingViewModel.GetSettingQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Commands.UI.Villages.VillageSettingViewModel.GetSettingQuery
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
		services.Add(new(typeof(global::MainCore.Commands.UI.Villages.VillageSettingViewModel.GetSettingQuery.Handler), typeof(global::MainCore.Commands.UI.Villages.VillageSettingViewModel.GetSettingQuery.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.Villages.VillageSettingViewModel.GetSettingQuery.Query, global::System.Collections.Generic.Dictionary<global::MainCore.Enums.VillageSettingEnums, int>>), typeof(global::MainCore.Commands.UI.Villages.VillageSettingViewModel.GetSettingQuery.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.UI.Villages.VillageSettingViewModel.GetSettingQuery.HandleBehavior), typeof(global::MainCore.Commands.UI.Villages.VillageSettingViewModel.GetSettingQuery.HandleBehavior), lifetime));
		return services;
	}
}
