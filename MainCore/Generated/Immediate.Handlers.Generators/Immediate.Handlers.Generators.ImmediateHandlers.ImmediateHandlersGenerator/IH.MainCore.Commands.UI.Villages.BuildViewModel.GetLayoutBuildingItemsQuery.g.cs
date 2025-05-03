using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.UI.Villages.BuildViewModel;

partial class GetLayoutBuildingItemsQuery
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.Villages.BuildViewModel.GetLayoutBuildingItemsQuery.Query, global::System.Collections.Generic.List<global::MainCore.UI.Models.Output.ListBoxItem>>
	{
		private readonly global::MainCore.Commands.UI.Villages.BuildViewModel.GetLayoutBuildingItemsQuery.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Commands.UI.Villages.BuildViewModel.GetLayoutBuildingItemsQuery.Query, global::System.Collections.Generic.List<global::MainCore.UI.Models.Output.ListBoxItem>> _loggingBehavior;

		public Handler(
			global::MainCore.Commands.UI.Villages.BuildViewModel.GetLayoutBuildingItemsQuery.HandleBehavior handleBehavior,
			global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Commands.UI.Villages.BuildViewModel.GetLayoutBuildingItemsQuery.Query, global::System.Collections.Generic.List<global::MainCore.UI.Models.Output.ListBoxItem>> loggingBehavior
		)
		{
			var handlerType = typeof(GetLayoutBuildingItemsQuery);

			_handleBehavior = handleBehavior;

			_loggingBehavior = loggingBehavior;
			_loggingBehavior.HandlerType = handlerType;

			_loggingBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::System.Collections.Generic.List<global::MainCore.UI.Models.Output.ListBoxItem>> HandleAsync(
			global::MainCore.Commands.UI.Villages.BuildViewModel.GetLayoutBuildingItemsQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _loggingBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.UI.Villages.BuildViewModel.GetLayoutBuildingItemsQuery.Query, global::System.Collections.Generic.List<global::MainCore.UI.Models.Output.ListBoxItem>>
	{
		private readonly global::MainCore.Commands.Queries.GetLayoutBuildingsQuery.Handler _getLayoutBuildingsQuery;

		public HandleBehavior(
			global::MainCore.Commands.Queries.GetLayoutBuildingsQuery.Handler getLayoutBuildingsQuery
		)
		{
			_getLayoutBuildingsQuery = getLayoutBuildingsQuery;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.Collections.Generic.List<global::MainCore.UI.Models.Output.ListBoxItem>> HandleAsync(
			global::MainCore.Commands.UI.Villages.BuildViewModel.GetLayoutBuildingItemsQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Commands.UI.Villages.BuildViewModel.GetLayoutBuildingItemsQuery
				.HandleAsync(
					request
					, _getLayoutBuildingsQuery
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
		services.Add(new(typeof(global::MainCore.Commands.UI.Villages.BuildViewModel.GetLayoutBuildingItemsQuery.Handler), typeof(global::MainCore.Commands.UI.Villages.BuildViewModel.GetLayoutBuildingItemsQuery.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.Villages.BuildViewModel.GetLayoutBuildingItemsQuery.Query, global::System.Collections.Generic.List<global::MainCore.UI.Models.Output.ListBoxItem>>), typeof(global::MainCore.Commands.UI.Villages.BuildViewModel.GetLayoutBuildingItemsQuery.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.UI.Villages.BuildViewModel.GetLayoutBuildingItemsQuery.HandleBehavior), typeof(global::MainCore.Commands.UI.Villages.BuildViewModel.GetLayoutBuildingItemsQuery.HandleBehavior), lifetime));
		return services;
	}
}
