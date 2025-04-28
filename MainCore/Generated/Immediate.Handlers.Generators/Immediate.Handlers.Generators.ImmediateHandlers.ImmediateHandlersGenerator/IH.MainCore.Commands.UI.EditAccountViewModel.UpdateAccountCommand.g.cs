using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.UI.EditAccountViewModel;

partial class UpdateAccountCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.EditAccountViewModel.UpdateAccountCommand.Command, global::System.ValueTuple>
	{
		private readonly global::MainCore.Commands.UI.EditAccountViewModel.UpdateAccountCommand.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Commands.UI.EditAccountViewModel.UpdateAccountCommand.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(UpdateAccountCommand);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.UI.EditAccountViewModel.UpdateAccountCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.UI.EditAccountViewModel.UpdateAccountCommand.Command, global::System.ValueTuple>
	{
		private readonly global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> _contextFactory;
		private readonly global::MainCore.Services.IUseragentManager _useragentManager;
		private readonly global::MainCore.Notification.Message.AccountUpdated.Handler _accountUpdated;

		public HandleBehavior(
			global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> contextFactory,
			global::MainCore.Services.IUseragentManager useragentManager,
			global::MainCore.Notification.Message.AccountUpdated.Handler accountUpdated
		)
		{
			_contextFactory = contextFactory;
			_useragentManager = useragentManager;
			_accountUpdated = accountUpdated;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.UI.EditAccountViewModel.UpdateAccountCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Commands.UI.EditAccountViewModel.UpdateAccountCommand
				.HandleAsync(
					request
					, _contextFactory
					, _useragentManager
					, _accountUpdated
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
