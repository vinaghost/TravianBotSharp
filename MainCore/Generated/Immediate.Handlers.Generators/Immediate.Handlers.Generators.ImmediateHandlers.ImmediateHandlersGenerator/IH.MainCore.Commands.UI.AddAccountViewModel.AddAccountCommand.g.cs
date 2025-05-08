using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.UI.AddAccountViewModel;

partial class AddAccountCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.AddAccountViewModel.AddAccountCommand.Command, global::FluentResults.Result>
	{
		private readonly global::MainCore.Commands.UI.AddAccountViewModel.AddAccountCommand.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Notifications.Behaviors.AccountListUpdatedBehavior<global::MainCore.Commands.UI.AddAccountViewModel.AddAccountCommand.Command, global::FluentResults.Result> _accountListUpdatedBehavior;

		public Handler(
			global::MainCore.Commands.UI.AddAccountViewModel.AddAccountCommand.HandleBehavior handleBehavior,
			global::MainCore.Notifications.Behaviors.AccountListUpdatedBehavior<global::MainCore.Commands.UI.AddAccountViewModel.AddAccountCommand.Command, global::FluentResults.Result> accountListUpdatedBehavior
		)
		{
			var handlerType = typeof(AddAccountCommand);

			_handleBehavior = handleBehavior;

			_accountListUpdatedBehavior = accountListUpdatedBehavior;
			_accountListUpdatedBehavior.HandlerType = handlerType;

			_accountListUpdatedBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Commands.UI.AddAccountViewModel.AddAccountCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _accountListUpdatedBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.UI.AddAccountViewModel.AddAccountCommand.Command, global::FluentResults.Result>
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

		public override async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Commands.UI.AddAccountViewModel.AddAccountCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Commands.UI.AddAccountViewModel.AddAccountCommand
				.HandleAsync(
					request
					, _context
					, _useragentManager
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
		services.Add(new(typeof(global::MainCore.Commands.UI.AddAccountViewModel.AddAccountCommand.Handler), typeof(global::MainCore.Commands.UI.AddAccountViewModel.AddAccountCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.AddAccountViewModel.AddAccountCommand.Command, global::FluentResults.Result>), typeof(global::MainCore.Commands.UI.AddAccountViewModel.AddAccountCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.UI.AddAccountViewModel.AddAccountCommand.HandleBehavior), typeof(global::MainCore.Commands.UI.AddAccountViewModel.AddAccountCommand.HandleBehavior), lifetime));
		return services;
	}
}
