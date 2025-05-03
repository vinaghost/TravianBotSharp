using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.UI.MainLayoutViewModel;

partial class GetAccountItemsQuery
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.MainLayoutViewModel.GetAccountItemsQuery.Query, global::System.Collections.Generic.List<global::MainCore.UI.Models.Output.ListBoxItem>>
	{
		private readonly global::MainCore.Commands.UI.MainLayoutViewModel.GetAccountItemsQuery.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Commands.UI.MainLayoutViewModel.GetAccountItemsQuery.Query, global::System.Collections.Generic.List<global::MainCore.UI.Models.Output.ListBoxItem>> _loggingBehavior;

		public Handler(
			global::MainCore.Commands.UI.MainLayoutViewModel.GetAccountItemsQuery.HandleBehavior handleBehavior,
			global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Commands.UI.MainLayoutViewModel.GetAccountItemsQuery.Query, global::System.Collections.Generic.List<global::MainCore.UI.Models.Output.ListBoxItem>> loggingBehavior
		)
		{
			var handlerType = typeof(GetAccountItemsQuery);

			_handleBehavior = handleBehavior;

			_loggingBehavior = loggingBehavior;
			_loggingBehavior.HandlerType = handlerType;

			_loggingBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::System.Collections.Generic.List<global::MainCore.UI.Models.Output.ListBoxItem>> HandleAsync(
			global::MainCore.Commands.UI.MainLayoutViewModel.GetAccountItemsQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _loggingBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.UI.MainLayoutViewModel.GetAccountItemsQuery.Query, global::System.Collections.Generic.List<global::MainCore.UI.Models.Output.ListBoxItem>>
	{
		private readonly global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> _contextFactory;
		private readonly global::MainCore.Services.ITaskManager _taskManager;

		public HandleBehavior(
			global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> contextFactory,
			global::MainCore.Services.ITaskManager taskManager
		)
		{
			_contextFactory = contextFactory;
			_taskManager = taskManager;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.Collections.Generic.List<global::MainCore.UI.Models.Output.ListBoxItem>> HandleAsync(
			global::MainCore.Commands.UI.MainLayoutViewModel.GetAccountItemsQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Commands.UI.MainLayoutViewModel.GetAccountItemsQuery
				.HandleAsync(
					request
					, _contextFactory
					, _taskManager
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
		services.Add(new(typeof(global::MainCore.Commands.UI.MainLayoutViewModel.GetAccountItemsQuery.Handler), typeof(global::MainCore.Commands.UI.MainLayoutViewModel.GetAccountItemsQuery.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.MainLayoutViewModel.GetAccountItemsQuery.Query, global::System.Collections.Generic.List<global::MainCore.UI.Models.Output.ListBoxItem>>), typeof(global::MainCore.Commands.UI.MainLayoutViewModel.GetAccountItemsQuery.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.UI.MainLayoutViewModel.GetAccountItemsQuery.HandleBehavior), typeof(global::MainCore.Commands.UI.MainLayoutViewModel.GetAccountItemsQuery.HandleBehavior), lifetime));
		return services;
	}
}
