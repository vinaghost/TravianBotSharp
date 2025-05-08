using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.UI.Misc;

partial class SaveVillageSettingCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.Misc.SaveVillageSettingCommand.Command, global::System.ValueTuple>
	{
		private readonly global::MainCore.Commands.UI.Misc.SaveVillageSettingCommand.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Notifications.Behaviors.VillageSettingUpdatedBehavior<global::MainCore.Commands.UI.Misc.SaveVillageSettingCommand.Command, global::System.ValueTuple> _villageSettingUpdatedBehavior;

		public Handler(
			global::MainCore.Commands.UI.Misc.SaveVillageSettingCommand.HandleBehavior handleBehavior,
			global::MainCore.Notifications.Behaviors.VillageSettingUpdatedBehavior<global::MainCore.Commands.UI.Misc.SaveVillageSettingCommand.Command, global::System.ValueTuple> villageSettingUpdatedBehavior
		)
		{
			var handlerType = typeof(SaveVillageSettingCommand);

			_handleBehavior = handleBehavior;

			_villageSettingUpdatedBehavior = villageSettingUpdatedBehavior;
			_villageSettingUpdatedBehavior.HandlerType = handlerType;

			_villageSettingUpdatedBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.UI.Misc.SaveVillageSettingCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _villageSettingUpdatedBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.UI.Misc.SaveVillageSettingCommand.Command, global::System.ValueTuple>
	{
		private readonly global::MainCore.Infrasturecture.Persistence.AppDbContext _context;

		public HandleBehavior(
			global::MainCore.Infrasturecture.Persistence.AppDbContext context
		)
		{
			_context = context;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.UI.Misc.SaveVillageSettingCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Commands.UI.Misc.SaveVillageSettingCommand
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
		services.Add(new(typeof(global::MainCore.Commands.UI.Misc.SaveVillageSettingCommand.Handler), typeof(global::MainCore.Commands.UI.Misc.SaveVillageSettingCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.Misc.SaveVillageSettingCommand.Command, global::System.ValueTuple>), typeof(global::MainCore.Commands.UI.Misc.SaveVillageSettingCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.UI.Misc.SaveVillageSettingCommand.HandleBehavior), typeof(global::MainCore.Commands.UI.Misc.SaveVillageSettingCommand.HandleBehavior), lifetime));
		return services;
	}
}
