using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.Update;

partial class UpdateFarmlistCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Update.UpdateFarmlistCommand.Command, global::FluentResults.Result>
	{
		private readonly global::MainCore.Commands.Update.UpdateFarmlistCommand.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Notification.Behaviors.FarmListUpdatedBehavior<global::MainCore.Commands.Update.UpdateFarmlistCommand.Command, global::FluentResults.Result> _farmListUpdatedBehavior;

		public Handler(
			global::MainCore.Commands.Update.UpdateFarmlistCommand.HandleBehavior handleBehavior,
			global::MainCore.Notification.Behaviors.FarmListUpdatedBehavior<global::MainCore.Commands.Update.UpdateFarmlistCommand.Command, global::FluentResults.Result> farmListUpdatedBehavior
		)
		{
			var handlerType = typeof(UpdateFarmlistCommand);

			_handleBehavior = handleBehavior;

			_farmListUpdatedBehavior = farmListUpdatedBehavior;
			_farmListUpdatedBehavior.HandlerType = handlerType;

			_farmListUpdatedBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Commands.Update.UpdateFarmlistCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _farmListUpdatedBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.Update.UpdateFarmlistCommand.Command, global::FluentResults.Result>
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
			global::MainCore.Commands.Update.UpdateFarmlistCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Commands.Update.UpdateFarmlistCommand
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
		services.Add(new(typeof(global::MainCore.Commands.Update.UpdateFarmlistCommand.Handler), typeof(global::MainCore.Commands.Update.UpdateFarmlistCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Update.UpdateFarmlistCommand.Command, global::FluentResults.Result>), typeof(global::MainCore.Commands.Update.UpdateFarmlistCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.Update.UpdateFarmlistCommand.HandleBehavior), typeof(global::MainCore.Commands.Update.UpdateFarmlistCommand.HandleBehavior), lifetime));
		return services;
	}
}
