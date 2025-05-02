using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.Features.StartFarmList;

partial class StartActiveFarmListCommand
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Features.StartFarmList.StartActiveFarmListCommand.Command, global::FluentResults.Result>
	{
		private readonly global::MainCore.Commands.Features.StartFarmList.StartActiveFarmListCommand.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Commands.Features.StartFarmList.StartActiveFarmListCommand.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(StartActiveFarmListCommand);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Commands.Features.StartFarmList.StartActiveFarmListCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.Features.StartFarmList.StartActiveFarmListCommand.Command, global::FluentResults.Result>
	{
		private readonly global::MainCore.Services.IChromeManager _chromeManager;
		private readonly global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> _contextFactory;
		private readonly global::MainCore.Commands.Misc.DelayClickCommand.Handler _delayClickCommand;

		public HandleBehavior(
			global::MainCore.Services.IChromeManager chromeManager,
			global::Microsoft.EntityFrameworkCore.IDbContextFactory<global::MainCore.Infrasturecture.Persistence.AppDbContext> contextFactory,
			global::MainCore.Commands.Misc.DelayClickCommand.Handler delayClickCommand
		)
		{
			_chromeManager = chromeManager;
			_contextFactory = contextFactory;
			_delayClickCommand = delayClickCommand;
		}

		public override async global::System.Threading.Tasks.ValueTask<global::FluentResults.Result> HandleAsync(
			global::MainCore.Commands.Features.StartFarmList.StartActiveFarmListCommand.Command request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Commands.Features.StartFarmList.StartActiveFarmListCommand
				.HandleAsync(
					request
					, _chromeManager
					, _contextFactory
					, _delayClickCommand
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
		services.Add(new(typeof(global::MainCore.Commands.Features.StartFarmList.StartActiveFarmListCommand.Handler), typeof(global::MainCore.Commands.Features.StartFarmList.StartActiveFarmListCommand.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.Features.StartFarmList.StartActiveFarmListCommand.Command, global::FluentResults.Result>), typeof(global::MainCore.Commands.Features.StartFarmList.StartActiveFarmListCommand.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.Features.StartFarmList.StartActiveFarmListCommand.HandleBehavior), typeof(global::MainCore.Commands.Features.StartFarmList.StartActiveFarmListCommand.HandleBehavior), lifetime));
		return services;
	}
}
