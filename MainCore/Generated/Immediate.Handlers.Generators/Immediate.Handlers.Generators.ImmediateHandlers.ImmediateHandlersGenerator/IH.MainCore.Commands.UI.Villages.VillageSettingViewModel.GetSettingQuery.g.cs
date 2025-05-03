using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.UI.Villages.VillageSettingViewModel;

partial class GetSettingQuery
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.Villages.VillageSettingViewModel.GetSettingQuery.Query, global::System.Collections.Generic.Dictionary<global::MainCore.Common.Enums.VillageSettingEnums, int>>
	{
		private readonly global::MainCore.Commands.UI.Villages.VillageSettingViewModel.GetSettingQuery.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Commands.UI.Villages.VillageSettingViewModel.GetSettingQuery.Query, global::System.Collections.Generic.Dictionary<global::MainCore.Common.Enums.VillageSettingEnums, int>> _loggingBehavior;

		public Handler(
			global::MainCore.Commands.UI.Villages.VillageSettingViewModel.GetSettingQuery.HandleBehavior handleBehavior,
			global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Commands.UI.Villages.VillageSettingViewModel.GetSettingQuery.Query, global::System.Collections.Generic.Dictionary<global::MainCore.Common.Enums.VillageSettingEnums, int>> loggingBehavior
		)
		{
			var handlerType = typeof(GetSettingQuery);

			_handleBehavior = handleBehavior;

			_loggingBehavior = loggingBehavior;
			_loggingBehavior.HandlerType = handlerType;

			_loggingBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::System.Collections.Generic.Dictionary<global::MainCore.Common.Enums.VillageSettingEnums, int>> HandleAsync(
			global::MainCore.Commands.UI.Villages.VillageSettingViewModel.GetSettingQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _loggingBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.UI.Villages.VillageSettingViewModel.GetSettingQuery.Query, global::System.Collections.Generic.Dictionary<global::MainCore.Common.Enums.VillageSettingEnums, int>>
	{
		private readonly global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> _contextFactory;

		public HandleBehavior(
			global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> contextFactory
		)
		{
			_contextFactory = contextFactory;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.Collections.Generic.Dictionary<global::MainCore.Common.Enums.VillageSettingEnums, int>> HandleAsync(
			global::MainCore.Commands.UI.Villages.VillageSettingViewModel.GetSettingQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Commands.UI.Villages.VillageSettingViewModel.GetSettingQuery
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
		services.Add(new(typeof(global::MainCore.Commands.UI.Villages.VillageSettingViewModel.GetSettingQuery.Handler), typeof(global::MainCore.Commands.UI.Villages.VillageSettingViewModel.GetSettingQuery.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.Villages.VillageSettingViewModel.GetSettingQuery.Query, global::System.Collections.Generic.Dictionary<global::MainCore.Common.Enums.VillageSettingEnums, int>>), typeof(global::MainCore.Commands.UI.Villages.VillageSettingViewModel.GetSettingQuery.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.UI.Villages.VillageSettingViewModel.GetSettingQuery.HandleBehavior), typeof(global::MainCore.Commands.UI.Villages.VillageSettingViewModel.GetSettingQuery.HandleBehavior), lifetime));
		return services;
	}
}
