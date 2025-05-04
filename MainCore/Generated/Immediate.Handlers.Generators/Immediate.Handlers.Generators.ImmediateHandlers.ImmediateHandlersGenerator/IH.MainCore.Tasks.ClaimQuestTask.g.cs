using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Tasks;

partial class ClaimQuestTask
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Tasks.ClaimQuestTask.Task, global::FluentResults.Result>
	{
		private readonly global::MainCore.Tasks.ClaimQuestTask.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Tasks.Behaviors.VillageTaskBehavior<global::MainCore.Tasks.ClaimQuestTask.Task, global::FluentResults.Result> _villageTaskBehavior;
		private readonly global::MainCore.Tasks.Behaviors.AccountTaskBehavior<global::MainCore.Tasks.ClaimQuestTask.Task, global::FluentResults.Result> _accountTaskBehavior;

		public Handler(
			global::MainCore.Tasks.ClaimQuestTask.HandleBehavior handleBehavior,
			global::MainCore.Tasks.Behaviors.VillageTaskBehavior<global::MainCore.Tasks.ClaimQuestTask.Task, global::FluentResults.Result> villageTaskBehavior,
			global::MainCore.Tasks.Behaviors.AccountTaskBehavior<global::MainCore.Tasks.ClaimQuestTask.Task, global::FluentResults.Result> accountTaskBehavior
		)
		{
			var handlerType = typeof(ClaimQuestTask);

			_handleBehavior = handleBehavior;

			_accountTaskBehavior = accountTaskBehavior;
			_accountTaskBehavior.HandlerType = handlerType;

			_villageTaskBehavior = villageTaskBehavior;
			_villageTaskBehavior.HandlerType = handlerType;

			_villageTaskBehavior.SetInnerHandler(_handleBehavior);
			_accountTaskBehavior.SetInnerHandler(_villageTaskBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Tasks.ClaimQuestTask.Task request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _accountTaskBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Tasks.ClaimQuestTask.Task, global::FluentResults.Result>
	{
		private readonly global::MainCore.Commands.Features.ClaimQuest.ToQuestPageCommand.Handler _toQuestPageCommand;
		private readonly global::MainCore.Commands.Features.ClaimQuest.ClaimQuestCommand.Handler _claimQuestCommand;

		public HandleBehavior(
			global::MainCore.Commands.Features.ClaimQuest.ToQuestPageCommand.Handler toQuestPageCommand,
			global::MainCore.Commands.Features.ClaimQuest.ClaimQuestCommand.Handler claimQuestCommand
		)
		{
			_toQuestPageCommand = toQuestPageCommand;
			_claimQuestCommand = claimQuestCommand;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Tasks.ClaimQuestTask.Task request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Tasks.ClaimQuestTask
				.HandleAsync(
					request
					, _toQuestPageCommand
					, _claimQuestCommand
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
		services.Add(new(typeof(global::MainCore.Tasks.ClaimQuestTask.Handler), typeof(global::MainCore.Tasks.ClaimQuestTask.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Tasks.ClaimQuestTask.Task, global::FluentResults.Result>), typeof(global::MainCore.Tasks.ClaimQuestTask.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Tasks.ClaimQuestTask.HandleBehavior), typeof(global::MainCore.Tasks.ClaimQuestTask.HandleBehavior), lifetime));
		return services;
	}
}
