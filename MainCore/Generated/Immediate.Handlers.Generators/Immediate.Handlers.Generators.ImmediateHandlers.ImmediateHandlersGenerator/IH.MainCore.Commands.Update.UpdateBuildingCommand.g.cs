using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.Update;

partial class UpdateBuildingCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Update.UpdateBuildingCommand.Command, global::FluentResults.Result<global::MainCore.Commands.Update.UpdateBuildingCommand.Response>>
	{
		private readonly global::MainCore.Commands.Update.UpdateBuildingCommand.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Notification.Behaviors.BuildingUpdatedBehavior<global::MainCore.Commands.Update.UpdateBuildingCommand.Command, global::FluentResults.Result<global::MainCore.Commands.Update.UpdateBuildingCommand.Response>> _buildingUpdatedBehavior;

		public Handler(
			global::MainCore.Commands.Update.UpdateBuildingCommand.HandleBehavior handleBehavior,
			global::MainCore.Notification.Behaviors.BuildingUpdatedBehavior<global::MainCore.Commands.Update.UpdateBuildingCommand.Command, global::FluentResults.Result<global::MainCore.Commands.Update.UpdateBuildingCommand.Response>> buildingUpdatedBehavior
		)
		{
			var handlerType = typeof(UpdateBuildingCommand);

			_handleBehavior = handleBehavior;

			_buildingUpdatedBehavior = buildingUpdatedBehavior;
			_buildingUpdatedBehavior.HandlerType = handlerType;

			_buildingUpdatedBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result<global::MainCore.Commands.Update.UpdateBuildingCommand.Response>> HandleAsync(
			global::MainCore.Commands.Update.UpdateBuildingCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _buildingUpdatedBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.Update.UpdateBuildingCommand.Command, global::FluentResults.Result<global::MainCore.Commands.Update.UpdateBuildingCommand.Response>>
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

		public override async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result<global::MainCore.Commands.Update.UpdateBuildingCommand.Response>> HandleAsync(
			global::MainCore.Commands.Update.UpdateBuildingCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Commands.Update.UpdateBuildingCommand
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
		services.Add(new(typeof(global::MainCore.Commands.Update.UpdateBuildingCommand.Handler), typeof(global::MainCore.Commands.Update.UpdateBuildingCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Update.UpdateBuildingCommand.Command, global::FluentResults.Result<global::MainCore.Commands.Update.UpdateBuildingCommand.Response>>), typeof(global::MainCore.Commands.Update.UpdateBuildingCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.Update.UpdateBuildingCommand.HandleBehavior), typeof(global::MainCore.Commands.Update.UpdateBuildingCommand.HandleBehavior), lifetime));
		return services;
	}
}
