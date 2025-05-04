using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Tasks;

partial class CompleteImmediatelyTask
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Tasks.CompleteImmediatelyTask.Task, global::FluentResults.Result>
	{
		private readonly global::MainCore.Tasks.CompleteImmediatelyTask.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Tasks.Behaviors.VillageTaskBehavior<global::MainCore.Tasks.CompleteImmediatelyTask.Task, global::FluentResults.Result> _villageTaskBehavior;
		private readonly global::MainCore.Tasks.Behaviors.AccountTaskBehavior<global::MainCore.Tasks.CompleteImmediatelyTask.Task, global::FluentResults.Result> _accountTaskBehavior;

		public Handler(
			global::MainCore.Tasks.CompleteImmediatelyTask.HandleBehavior handleBehavior,
			global::MainCore.Tasks.Behaviors.VillageTaskBehavior<global::MainCore.Tasks.CompleteImmediatelyTask.Task, global::FluentResults.Result> villageTaskBehavior,
			global::MainCore.Tasks.Behaviors.AccountTaskBehavior<global::MainCore.Tasks.CompleteImmediatelyTask.Task, global::FluentResults.Result> accountTaskBehavior
		)
		{
			var handlerType = typeof(CompleteImmediatelyTask);

			_handleBehavior = handleBehavior;

			_accountTaskBehavior = accountTaskBehavior;
			_accountTaskBehavior.HandlerType = handlerType;

			_villageTaskBehavior = villageTaskBehavior;
			_villageTaskBehavior.HandlerType = handlerType;

			_villageTaskBehavior.SetInnerHandler(_handleBehavior);
			_accountTaskBehavior.SetInnerHandler(_villageTaskBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Tasks.CompleteImmediatelyTask.Task request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _accountTaskBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Tasks.CompleteImmediatelyTask.Task, global::FluentResults.Result>
	{
		private readonly global::MainCore.Commands.Navigate.ToDorfCommand.Handler _toDorfCommand;
		private readonly global::MainCore.Commands.Features.CompleteImmediately.CompleteImmediatelyCommand.Handler _completeImmediatelyCommand;
		private readonly global::MainCore.Commands.Update.UpdateBuildingCommand.Handler _updateBuildingCommand;

		public HandleBehavior(
			global::MainCore.Commands.Navigate.ToDorfCommand.Handler toDorfCommand,
			global::MainCore.Commands.Features.CompleteImmediately.CompleteImmediatelyCommand.Handler completeImmediatelyCommand,
			global::MainCore.Commands.Update.UpdateBuildingCommand.Handler updateBuildingCommand
		)
		{
			_toDorfCommand = toDorfCommand;
			_completeImmediatelyCommand = completeImmediatelyCommand;
			_updateBuildingCommand = updateBuildingCommand;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Tasks.CompleteImmediatelyTask.Task request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Tasks.CompleteImmediatelyTask
				.HandleAsync(
					request
					, _toDorfCommand
					, _completeImmediatelyCommand
					, _updateBuildingCommand
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
		services.Add(new(typeof(global::MainCore.Tasks.CompleteImmediatelyTask.Handler), typeof(global::MainCore.Tasks.CompleteImmediatelyTask.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Tasks.CompleteImmediatelyTask.Task, global::FluentResults.Result>), typeof(global::MainCore.Tasks.CompleteImmediatelyTask.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Tasks.CompleteImmediatelyTask.HandleBehavior), typeof(global::MainCore.Tasks.CompleteImmediatelyTask.HandleBehavior), lifetime));
		return services;
	}
}
