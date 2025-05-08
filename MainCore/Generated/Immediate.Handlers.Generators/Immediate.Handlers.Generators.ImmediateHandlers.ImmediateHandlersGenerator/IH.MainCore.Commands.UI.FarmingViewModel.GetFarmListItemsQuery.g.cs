using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.UI.FarmingViewModel;

partial class GetFarmListItemsQuery
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.FarmingViewModel.GetFarmListItemsQuery.Query, global::System.Collections.Generic.List<global::MainCore.UI.Models.Output.ListBoxItem>>
	{
		private readonly global::MainCore.Commands.UI.FarmingViewModel.GetFarmListItemsQuery.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Commands.UI.FarmingViewModel.GetFarmListItemsQuery.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(GetFarmListItemsQuery);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.Collections.Generic.List<global::MainCore.UI.Models.Output.ListBoxItem>> HandleAsync(
			global::MainCore.Commands.UI.FarmingViewModel.GetFarmListItemsQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.UI.FarmingViewModel.GetFarmListItemsQuery.Query, global::System.Collections.Generic.List<global::MainCore.UI.Models.Output.ListBoxItem>>
	{
		private readonly global::MainCore.Infrasturecture.Persistence.AppDbContext _context;

		public HandleBehavior(
			global::MainCore.Infrasturecture.Persistence.AppDbContext context
		)
		{
			_context = context;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.Collections.Generic.List<global::MainCore.UI.Models.Output.ListBoxItem>> HandleAsync(
			global::MainCore.Commands.UI.FarmingViewModel.GetFarmListItemsQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Commands.UI.FarmingViewModel.GetFarmListItemsQuery
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
		services.Add(new(typeof(global::MainCore.Commands.UI.FarmingViewModel.GetFarmListItemsQuery.Handler), typeof(global::MainCore.Commands.UI.FarmingViewModel.GetFarmListItemsQuery.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.FarmingViewModel.GetFarmListItemsQuery.Query, global::System.Collections.Generic.List<global::MainCore.UI.Models.Output.ListBoxItem>>), typeof(global::MainCore.Commands.UI.FarmingViewModel.GetFarmListItemsQuery.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.UI.FarmingViewModel.GetFarmListItemsQuery.HandleBehavior), typeof(global::MainCore.Commands.UI.FarmingViewModel.GetFarmListItemsQuery.HandleBehavior), lifetime));
		return services;
	}
}
