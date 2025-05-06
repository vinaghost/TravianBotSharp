using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.UI.Misc;

partial class SaveVillageSettingCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.Misc.SaveVillageSettingCommand.Command, global::System.ValueTuple>
	{
		private readonly global::MainCore.Commands.UI.Misc.SaveVillageSettingCommand.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Commands.UI.Misc.SaveVillageSettingCommand.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(SaveVillageSettingCommand);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.UI.Misc.SaveVillageSettingCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.UI.Misc.SaveVillageSettingCommand.Command, global::System.ValueTuple>
	{
		private readonly global::MainCore.Infrasturecture.Persistence.AppDbContext _context;
		private readonly global::MainCore.Notification.Message.VillageSettingUpdated.Handler _villageSettingUpdated;

		public HandleBehavior(
			global::MainCore.Infrasturecture.Persistence.AppDbContext context,
			global::MainCore.Notification.Message.VillageSettingUpdated.Handler villageSettingUpdated
		)
		{
			_context = context;
			_villageSettingUpdated = villageSettingUpdated;
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
					, _villageSettingUpdated
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
