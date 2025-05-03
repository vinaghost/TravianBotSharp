using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.UI.VillageViewModel;

partial class GetVillageItemsQuery
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.VillageViewModel.GetVillageItemsQuery.Query, global::System.Collections.Generic.List<global::MainCore.UI.Models.Output.ListBoxItem>>
	{
		private readonly global::MainCore.Commands.UI.VillageViewModel.GetVillageItemsQuery.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Commands.UI.VillageViewModel.GetVillageItemsQuery.Query, global::System.Collections.Generic.List<global::MainCore.UI.Models.Output.ListBoxItem>> _loggingBehavior;

		public Handler(
			global::MainCore.Commands.UI.VillageViewModel.GetVillageItemsQuery.HandleBehavior handleBehavior,
			global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Commands.UI.VillageViewModel.GetVillageItemsQuery.Query, global::System.Collections.Generic.List<global::MainCore.UI.Models.Output.ListBoxItem>> loggingBehavior
		)
		{
			var handlerType = typeof(GetVillageItemsQuery);

			_handleBehavior = handleBehavior;

			_loggingBehavior = loggingBehavior;
			_loggingBehavior.HandlerType = handlerType;

			_loggingBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::System.Collections.Generic.List<global::MainCore.UI.Models.Output.ListBoxItem>> HandleAsync(
			global::MainCore.Commands.UI.VillageViewModel.GetVillageItemsQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _loggingBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.UI.VillageViewModel.GetVillageItemsQuery.Query, global::System.Collections.Generic.List<global::MainCore.UI.Models.Output.ListBoxItem>>
	{
		private readonly global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> _contextFactory;

		public HandleBehavior(
			global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> contextFactory
		)
		{
			_contextFactory = contextFactory;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.Collections.Generic.List<global::MainCore.UI.Models.Output.ListBoxItem>> HandleAsync(
			global::MainCore.Commands.UI.VillageViewModel.GetVillageItemsQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Commands.UI.VillageViewModel.GetVillageItemsQuery
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
		services.Add(new(typeof(global::MainCore.Commands.UI.VillageViewModel.GetVillageItemsQuery.Handler), typeof(global::MainCore.Commands.UI.VillageViewModel.GetVillageItemsQuery.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.VillageViewModel.GetVillageItemsQuery.Query, global::System.Collections.Generic.List<global::MainCore.UI.Models.Output.ListBoxItem>>), typeof(global::MainCore.Commands.UI.VillageViewModel.GetVillageItemsQuery.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.UI.VillageViewModel.GetVillageItemsQuery.HandleBehavior), typeof(global::MainCore.Commands.UI.VillageViewModel.GetVillageItemsQuery.HandleBehavior), lifetime));
		return services;
	}
}
