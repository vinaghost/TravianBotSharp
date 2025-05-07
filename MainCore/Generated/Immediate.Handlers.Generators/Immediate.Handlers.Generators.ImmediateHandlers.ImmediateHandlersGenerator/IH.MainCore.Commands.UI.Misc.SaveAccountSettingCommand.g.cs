using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.UI.Misc;

partial class SaveAccountSettingCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.Misc.SaveAccountSettingCommand.Command, global::System.ValueTuple>
	{
		private readonly global::MainCore.Commands.UI.Misc.SaveAccountSettingCommand.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Notification.Behaviors.AccountSettingUpdatedBehavior<global::MainCore.Commands.UI.Misc.SaveAccountSettingCommand.Command, global::System.ValueTuple> _accountSettingUpdatedBehavior;

		public Handler(
			global::MainCore.Commands.UI.Misc.SaveAccountSettingCommand.HandleBehavior handleBehavior,
			global::MainCore.Notification.Behaviors.AccountSettingUpdatedBehavior<global::MainCore.Commands.UI.Misc.SaveAccountSettingCommand.Command, global::System.ValueTuple> accountSettingUpdatedBehavior
		)
		{
			var handlerType = typeof(SaveAccountSettingCommand);

			_handleBehavior = handleBehavior;

			_accountSettingUpdatedBehavior = accountSettingUpdatedBehavior;
			_accountSettingUpdatedBehavior.HandlerType = handlerType;

			_accountSettingUpdatedBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.UI.Misc.SaveAccountSettingCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _accountSettingUpdatedBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.UI.Misc.SaveAccountSettingCommand.Command, global::System.ValueTuple>
	{
		private readonly global::MainCore.Infrasturecture.Persistence.AppDbContext _context;

		public HandleBehavior(
			global::MainCore.Infrasturecture.Persistence.AppDbContext context
		)
		{
			_context = context;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.UI.Misc.SaveAccountSettingCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Commands.UI.Misc.SaveAccountSettingCommand
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
		services.Add(new(typeof(global::MainCore.Commands.UI.Misc.SaveAccountSettingCommand.Handler), typeof(global::MainCore.Commands.UI.Misc.SaveAccountSettingCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.Misc.SaveAccountSettingCommand.Command, global::System.ValueTuple>), typeof(global::MainCore.Commands.UI.Misc.SaveAccountSettingCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.UI.Misc.SaveAccountSettingCommand.HandleBehavior), typeof(global::MainCore.Commands.UI.Misc.SaveAccountSettingCommand.HandleBehavior), lifetime));
		return services;
	}
}
