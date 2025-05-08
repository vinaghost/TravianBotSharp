using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.Update;

partial class UpdateAccountInfoCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Update.UpdateAccountInfoCommand.Command, global::FluentResults.Result>
	{
		private readonly global::MainCore.Commands.Update.UpdateAccountInfoCommand.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Notifications.Behaviors.AccountInfoUpdatedBehavior<global::MainCore.Commands.Update.UpdateAccountInfoCommand.Command, global::FluentResults.Result> _accountInfoUpdatedBehavior;

		public Handler(
			global::MainCore.Commands.Update.UpdateAccountInfoCommand.HandleBehavior handleBehavior,
			global::MainCore.Notifications.Behaviors.AccountInfoUpdatedBehavior<global::MainCore.Commands.Update.UpdateAccountInfoCommand.Command, global::FluentResults.Result> accountInfoUpdatedBehavior
		)
		{
			var handlerType = typeof(UpdateAccountInfoCommand);

			_handleBehavior = handleBehavior;

			_accountInfoUpdatedBehavior = accountInfoUpdatedBehavior;
			_accountInfoUpdatedBehavior.HandlerType = handlerType;

			_accountInfoUpdatedBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Commands.Update.UpdateAccountInfoCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _accountInfoUpdatedBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.Update.UpdateAccountInfoCommand.Command, global::FluentResults.Result>
	{
		private readonly global::MainCore.Services.IChromeBrowser _browser;
		private readonly global::MainCore.Infrasturecture.Persistence.AppDbContext _context;

		public HandleBehavior(
			global::MainCore.Services.IChromeBrowser browser,
			global::MainCore.Infrasturecture.Persistence.AppDbContext context
		)
		{
			_browser = browser;
			_context = context;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Commands.Update.UpdateAccountInfoCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Commands.Update.UpdateAccountInfoCommand
				.HandleAsync(
					request
					, _browser
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
		services.Add(new(typeof(global::MainCore.Commands.Update.UpdateAccountInfoCommand.Handler), typeof(global::MainCore.Commands.Update.UpdateAccountInfoCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Update.UpdateAccountInfoCommand.Command, global::FluentResults.Result>), typeof(global::MainCore.Commands.Update.UpdateAccountInfoCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.Update.UpdateAccountInfoCommand.HandleBehavior), typeof(global::MainCore.Commands.Update.UpdateAccountInfoCommand.HandleBehavior), lifetime));
		return services;
	}
}
