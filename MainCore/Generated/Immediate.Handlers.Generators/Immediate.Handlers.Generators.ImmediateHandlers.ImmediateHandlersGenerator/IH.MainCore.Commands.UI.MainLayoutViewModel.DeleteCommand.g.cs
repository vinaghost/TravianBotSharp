using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.UI.MainLayoutViewModel;

partial class DeleteCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.MainLayoutViewModel.DeleteCommand.Command, global::System.ValueTuple>
	{
		private readonly global::MainCore.Commands.UI.MainLayoutViewModel.DeleteCommand.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Commands.UI.MainLayoutViewModel.DeleteCommand.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(DeleteCommand);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.UI.MainLayoutViewModel.DeleteCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.UI.MainLayoutViewModel.DeleteCommand.Command, global::System.ValueTuple>
	{
		private readonly global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> _contextFactory;
		private readonly global::MainCore.Notification.Message.FarmListUpdated.Handler _farmListUpdated;

		public HandleBehavior(
			global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> contextFactory,
			global::MainCore.Notification.Message.FarmListUpdated.Handler farmListUpdated
		)
		{
			_contextFactory = contextFactory;
			_farmListUpdated = farmListUpdated;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.UI.MainLayoutViewModel.DeleteCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Commands.UI.MainLayoutViewModel.DeleteCommand
				.HandleAsync(
					request
					, _contextFactory
					, _farmListUpdated
					, cancellationToken
				)
				.ConfigureAwait(false);

			return default;
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public static IServiceCollection AddHandlers(
		IServiceCollection services,
		ServiceLifetime lifetime = ServiceLifetime.Scoped
	)
	{
		services.Add(new(typeof(global::MainCore.Commands.UI.MainLayoutViewModel.DeleteCommand.Handler), typeof(global::MainCore.Commands.UI.MainLayoutViewModel.DeleteCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.MainLayoutViewModel.DeleteCommand.Command, global::System.ValueTuple>), typeof(global::MainCore.Commands.UI.MainLayoutViewModel.DeleteCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.UI.MainLayoutViewModel.DeleteCommand.HandleBehavior), typeof(global::MainCore.Commands.UI.MainLayoutViewModel.DeleteCommand.HandleBehavior), lifetime));
		return services;
	}
}
