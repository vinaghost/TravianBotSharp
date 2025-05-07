using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.UI.MainLayoutViewModel;

partial class DeleteCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.MainLayoutViewModel.DeleteCommand.Command, global::System.ValueTuple>
	{
		private readonly global::MainCore.Commands.UI.MainLayoutViewModel.DeleteCommand.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Notifications.Behaviors.AccountListUpdatedBehavior<global::MainCore.Commands.UI.MainLayoutViewModel.DeleteCommand.Command, global::System.ValueTuple> _accountListUpdatedBehavior;

		public Handler(
			global::MainCore.Commands.UI.MainLayoutViewModel.DeleteCommand.HandleBehavior handleBehavior,
			global::MainCore.Notifications.Behaviors.AccountListUpdatedBehavior<global::MainCore.Commands.UI.MainLayoutViewModel.DeleteCommand.Command, global::System.ValueTuple> accountListUpdatedBehavior
		)
		{
			var handlerType = typeof(DeleteCommand);

			_handleBehavior = handleBehavior;

			_accountListUpdatedBehavior = accountListUpdatedBehavior;
			_accountListUpdatedBehavior.HandlerType = handlerType;

			_accountListUpdatedBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.UI.MainLayoutViewModel.DeleteCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _accountListUpdatedBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.UI.MainLayoutViewModel.DeleteCommand.Command, global::System.ValueTuple>
	{
		private readonly global::MainCore.Infrasturecture.Persistence.AppDbContext _context;

		public HandleBehavior(
			global::MainCore.Infrasturecture.Persistence.AppDbContext context
		)
		{
			_context = context;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.UI.MainLayoutViewModel.DeleteCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Commands.UI.MainLayoutViewModel.DeleteCommand
				.HandleAsync(
					request
					, _context
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
