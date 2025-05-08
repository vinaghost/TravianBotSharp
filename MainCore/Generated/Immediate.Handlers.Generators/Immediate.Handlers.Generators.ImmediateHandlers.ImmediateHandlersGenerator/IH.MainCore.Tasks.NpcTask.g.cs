using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Tasks;

partial class NpcTask
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Tasks.NpcTask.Task, global::FluentResults.Result>
	{
		private readonly global::MainCore.Tasks.NpcTask.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Tasks.Behaviors.VillageTaskBehavior<global::MainCore.Tasks.NpcTask.Task, global::FluentResults.Result> _villageTaskBehavior;
		private readonly global::MainCore.Tasks.Behaviors.AccountTaskBehavior<global::MainCore.Tasks.NpcTask.Task, global::FluentResults.Result> _accountTaskBehavior;
		private readonly global::MainCore.Commands.Behaviors.CommandLoggingBehavior<global::MainCore.Tasks.NpcTask.Task, global::FluentResults.Result> _commandLoggingBehavior;

		public Handler(
			global::MainCore.Tasks.NpcTask.HandleBehavior handleBehavior,
			global::MainCore.Tasks.Behaviors.VillageTaskBehavior<global::MainCore.Tasks.NpcTask.Task, global::FluentResults.Result> villageTaskBehavior,
			global::MainCore.Tasks.Behaviors.AccountTaskBehavior<global::MainCore.Tasks.NpcTask.Task, global::FluentResults.Result> accountTaskBehavior,
			global::MainCore.Commands.Behaviors.CommandLoggingBehavior<global::MainCore.Tasks.NpcTask.Task, global::FluentResults.Result> commandLoggingBehavior
		)
		{
			var handlerType = typeof(NpcTask);

			_handleBehavior = handleBehavior;

			_commandLoggingBehavior = commandLoggingBehavior;
			_commandLoggingBehavior.HandlerType = handlerType;

			_accountTaskBehavior = accountTaskBehavior;
			_accountTaskBehavior.HandlerType = handlerType;

			_villageTaskBehavior = villageTaskBehavior;
			_villageTaskBehavior.HandlerType = handlerType;

			_villageTaskBehavior.SetInnerHandler(_handleBehavior);
			_accountTaskBehavior.SetInnerHandler(_villageTaskBehavior);
			_commandLoggingBehavior.SetInnerHandler(_accountTaskBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Tasks.NpcTask.Task request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _commandLoggingBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Tasks.NpcTask.Task, global::FluentResults.Result>
	{
		private readonly global::MainCore.Commands.Features.NpcResource.ToNpcResourcePageCommand.Handler _toNpcResourcePageCommand;
		private readonly global::MainCore.Commands.Features.NpcResource.NpcResourceCommand.Handler _npcResourceCommand;
		private readonly global::MainCore.Commands.Update.UpdateStorageCommand.Handler _updateStorageCommand;

		public HandleBehavior(
			global::MainCore.Commands.Features.NpcResource.ToNpcResourcePageCommand.Handler toNpcResourcePageCommand,
			global::MainCore.Commands.Features.NpcResource.NpcResourceCommand.Handler npcResourceCommand,
			global::MainCore.Commands.Update.UpdateStorageCommand.Handler updateStorageCommand
		)
		{
			_toNpcResourcePageCommand = toNpcResourcePageCommand;
			_npcResourceCommand = npcResourceCommand;
			_updateStorageCommand = updateStorageCommand;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Tasks.NpcTask.Task request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Tasks.NpcTask
				.HandleAsync(
					request
					, _toNpcResourcePageCommand
					, _npcResourceCommand
					, _updateStorageCommand
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
		services.Add(new(typeof(global::MainCore.Tasks.NpcTask.Handler), typeof(global::MainCore.Tasks.NpcTask.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Tasks.NpcTask.Task, global::FluentResults.Result>), typeof(global::MainCore.Tasks.NpcTask.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Tasks.NpcTask.HandleBehavior), typeof(global::MainCore.Tasks.NpcTask.HandleBehavior), lifetime));
		return services;
	}
}
