using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.Misc;

partial class DeleteJobByIdCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Misc.DeleteJobByIdCommand.Command, global::System.ValueTuple>
	{
		private readonly global::MainCore.Commands.Misc.DeleteJobByIdCommand.HandleBehavior _handleBehavior;
		private readonly global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Commands.Misc.DeleteJobByIdCommand.Command, global::System.ValueTuple> _loggingBehavior;

		public Handler(
			global::MainCore.Commands.Misc.DeleteJobByIdCommand.HandleBehavior handleBehavior,
			global::MainCore.Tasks.Behaviors.LoggingBehavior<global::MainCore.Commands.Misc.DeleteJobByIdCommand.Command, global::System.ValueTuple> loggingBehavior
		)
		{
			var handlerType = typeof(DeleteJobByIdCommand);

			_handleBehavior = handleBehavior;

			_loggingBehavior = loggingBehavior;
			_loggingBehavior.HandlerType = handlerType;

			_loggingBehavior.SetInnerHandler(_handleBehavior);
		}

		public async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.Misc.DeleteJobByIdCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _loggingBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.Misc.DeleteJobByIdCommand.Command, global::System.ValueTuple>
	{
		private readonly global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> _contextFactory;
		private readonly global::MainCore.Notification.Message.JobUpdated.Handler _jobUpdated;

		public HandleBehavior(
			global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> contextFactory,
			global::MainCore.Notification.Message.JobUpdated.Handler jobUpdated
		)
		{
			_contextFactory = contextFactory;
			_jobUpdated = jobUpdated;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::System.ValueTuple> HandleAsync(
			global::MainCore.Commands.Misc.DeleteJobByIdCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			await global::MainCore.Commands.Misc.DeleteJobByIdCommand
				.HandleAsync(
					request
					, _contextFactory
					, _jobUpdated
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
		services.Add(new(typeof(global::MainCore.Commands.Misc.DeleteJobByIdCommand.Handler), typeof(global::MainCore.Commands.Misc.DeleteJobByIdCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Misc.DeleteJobByIdCommand.Command, global::System.ValueTuple>), typeof(global::MainCore.Commands.Misc.DeleteJobByIdCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.Misc.DeleteJobByIdCommand.HandleBehavior), typeof(global::MainCore.Commands.Misc.DeleteJobByIdCommand.HandleBehavior), lifetime));
		return services;
	}
}
