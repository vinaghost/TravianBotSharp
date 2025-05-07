using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.UI.EditAccountViewModel;

partial class UpdateAccountCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.EditAccountViewModel.UpdateAccountCommand.Command, global::System.ValueTuple>
	{
		private readonly global::MainCore.Commands.UI.EditAccountViewModel.UpdateAccountCommand.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Notifications.Behaviors.AccountListUpdatedBehavior<global::MainCore.Commands.UI.EditAccountViewModel.UpdateAccountCommand.Command, global::System.ValueTuple> _accountListUpdatedBehavior;

		public Handler(
			global::MainCore.Commands.UI.EditAccountViewModel.UpdateAccountCommand.HandleBehavior handleBehavior,
			global::MainCore.Notifications.Behaviors.AccountListUpdatedBehavior<global::MainCore.Commands.UI.EditAccountViewModel.UpdateAccountCommand.Command, global::System.ValueTuple> accountListUpdatedBehavior
		)
		{
			var handlerType = typeof(UpdateAccountCommand);

			_handleBehavior = handleBehavior;

			_accountListUpdatedBehavior = accountListUpdatedBehavior;
			_accountListUpdatedBehavior.HandlerType = handlerType;

			_accountListUpdatedBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.UI.EditAccountViewModel.UpdateAccountCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _accountListUpdatedBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.UI.EditAccountViewModel.UpdateAccountCommand.Command, global::System.ValueTuple>
	{
		private readonly global::MainCore.Infrasturecture.Persistence.AppDbContext _context;
		private readonly global::MainCore.Services.IUseragentManager _useragentManager;

		public HandleBehavior(
			global::MainCore.Infrasturecture.Persistence.AppDbContext context,
			global::MainCore.Services.IUseragentManager useragentManager
		)
		{
			_context = context;
			_useragentManager = useragentManager;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.UI.EditAccountViewModel.UpdateAccountCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Commands.UI.EditAccountViewModel.UpdateAccountCommand
				.HandleAsync(
					request
					, _context
					, _useragentManager
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
		services.Add(new(typeof(global::MainCore.Commands.UI.EditAccountViewModel.UpdateAccountCommand.Handler), typeof(global::MainCore.Commands.UI.EditAccountViewModel.UpdateAccountCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.EditAccountViewModel.UpdateAccountCommand.Command, global::System.ValueTuple>), typeof(global::MainCore.Commands.UI.EditAccountViewModel.UpdateAccountCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.UI.EditAccountViewModel.UpdateAccountCommand.HandleBehavior), typeof(global::MainCore.Commands.UI.EditAccountViewModel.UpdateAccountCommand.HandleBehavior), lifetime));
		return services;
	}
}
