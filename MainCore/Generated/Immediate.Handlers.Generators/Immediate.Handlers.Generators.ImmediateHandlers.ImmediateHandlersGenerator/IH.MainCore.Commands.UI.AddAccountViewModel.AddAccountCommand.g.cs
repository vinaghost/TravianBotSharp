using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.UI.AddAccountViewModel;

partial class AddAccountCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.AddAccountViewModel.AddAccountCommand.Command, global::FluentResults.Result>
	{
		private readonly global::MainCore.Commands.UI.AddAccountViewModel.AddAccountCommand.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Commands.Behaviors.CommandLoggingBehavior<global::MainCore.Commands.UI.AddAccountViewModel.AddAccountCommand.Command, global::FluentResults.Result> _commandLoggingBehavior;

		public Handler(
			global::MainCore.Commands.UI.AddAccountViewModel.AddAccountCommand.HandleBehavior handleBehavior,
			global::MainCore.Commands.Behaviors.CommandLoggingBehavior<global::MainCore.Commands.UI.AddAccountViewModel.AddAccountCommand.Command, global::FluentResults.Result> commandLoggingBehavior
		)
		{
			var handlerType = typeof(AddAccountCommand);

			_handleBehavior = handleBehavior;

			_commandLoggingBehavior = commandLoggingBehavior;
			_commandLoggingBehavior.HandlerType = handlerType;

			_commandLoggingBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Commands.UI.AddAccountViewModel.AddAccountCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _commandLoggingBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.UI.AddAccountViewModel.AddAccountCommand.Command, global::FluentResults.Result>
	{
		private readonly global::MainCore.Infrasturecture.Persistence.AppDbContext _context;
		private readonly global::MainCore.Services.IUseragentManager _useragentManager;
		private readonly global::MainCore.Notification.Message.AccountUpdated.Handler _accountUpdated;

		public HandleBehavior(
			global::MainCore.Infrasturecture.Persistence.AppDbContext context,
			global::MainCore.Services.IUseragentManager useragentManager,
			global::MainCore.Notification.Message.AccountUpdated.Handler accountUpdated
		)
		{
			_context = context;
			_useragentManager = useragentManager;
			_accountUpdated = accountUpdated;
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
					, _accountUpdated
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
