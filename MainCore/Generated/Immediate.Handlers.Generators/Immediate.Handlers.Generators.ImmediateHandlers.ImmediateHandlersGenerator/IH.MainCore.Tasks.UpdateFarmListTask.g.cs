using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Tasks;

partial class UpdateFarmListTask
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Tasks.UpdateFarmListTask.Task, global::FluentResults.Result>
	{
		private readonly global::MainCore.Tasks.UpdateFarmListTask.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Tasks.Behaviors.AccountTaskBehavior<global::MainCore.Tasks.UpdateFarmListTask.Task, global::FluentResults.Result> _accountTaskBehavior;
		private readonly global::MainCore.Commands.Behaviors.CommandLoggingBehavior<global::MainCore.Tasks.UpdateFarmListTask.Task, global::FluentResults.Result> _commandLoggingBehavior;

		public Handler(
			global::MainCore.Tasks.UpdateFarmListTask.HandleBehavior handleBehavior,
			global::MainCore.Tasks.Behaviors.AccountTaskBehavior<global::MainCore.Tasks.UpdateFarmListTask.Task, global::FluentResults.Result> accountTaskBehavior,
			global::MainCore.Commands.Behaviors.CommandLoggingBehavior<global::MainCore.Tasks.UpdateFarmListTask.Task, global::FluentResults.Result> commandLoggingBehavior
		)
		{
			var handlerType = typeof(UpdateFarmListTask);

			_handleBehavior = handleBehavior;

			_commandLoggingBehavior = commandLoggingBehavior;
			_commandLoggingBehavior.HandlerType = handlerType;

			_accountTaskBehavior = accountTaskBehavior;
			_accountTaskBehavior.HandlerType = handlerType;

			_accountTaskBehavior.SetInnerHandler(_handleBehavior);
			_commandLoggingBehavior.SetInnerHandler(_accountTaskBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Tasks.UpdateFarmListTask.Task request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _commandLoggingBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Tasks.UpdateFarmListTask.Task, global::FluentResults.Result>
	{
		private readonly global::MainCore.Commands.Features.StartFarmList.ToFarmListPageCommand.Handler _toFarmListPageCommand;
		private readonly global::MainCore.Commands.Update.UpdateFarmlistCommand.Handler _updateFarmlistCommand;
		private readonly global::MainCore.Notification.Message.FarmListUpdated.Handler _farmListUpdated;

		public HandleBehavior(
			global::MainCore.Commands.Features.StartFarmList.ToFarmListPageCommand.Handler toFarmListPageCommand,
			global::MainCore.Commands.Update.UpdateFarmlistCommand.Handler updateFarmlistCommand,
			global::MainCore.Notification.Message.FarmListUpdated.Handler farmListUpdated
		)
		{
			_toFarmListPageCommand = toFarmListPageCommand;
			_updateFarmlistCommand = updateFarmlistCommand;
			_farmListUpdated = farmListUpdated;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Tasks.UpdateFarmListTask.Task request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Tasks.UpdateFarmListTask
				.HandleAsync(
					request
					, _toFarmListPageCommand
					, _updateFarmlistCommand
					, _farmListUpdated
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
		services.Add(new(typeof(global::MainCore.Tasks.UpdateFarmListTask.Handler), typeof(global::MainCore.Tasks.UpdateFarmListTask.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Tasks.UpdateFarmListTask.Task, global::FluentResults.Result>), typeof(global::MainCore.Tasks.UpdateFarmListTask.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Tasks.UpdateFarmListTask.HandleBehavior), typeof(global::MainCore.Tasks.UpdateFarmListTask.HandleBehavior), lifetime));
		return services;
	}
}
