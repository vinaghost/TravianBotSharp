using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.UI.DebugViewModel;

partial class GetEndpointAdressQuery
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.DebugViewModel.GetEndpointAdressQuery.Query, string>
	{
		private readonly global::MainCore.Commands.UI.DebugViewModel.GetEndpointAdressQuery.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Commands.UI.DebugViewModel.GetEndpointAdressQuery.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(GetEndpointAdressQuery);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<string> HandleAsync(
			global::MainCore.Commands.UI.DebugViewModel.GetEndpointAdressQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.UI.DebugViewModel.GetEndpointAdressQuery.Query, string>
	{
		private readonly global::MainCore.Services.ITimerManager __timerManager;

		public HandleBehavior(
			global::MainCore.Services.ITimerManager _timerManager
		)
		{
			__timerManager = _timerManager;
		}

		public override async global::System.Threading.Tasks.ValueTask<string> HandleAsync(
			global::MainCore.Commands.UI.DebugViewModel.GetEndpointAdressQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Commands.UI.DebugViewModel.GetEndpointAdressQuery
				.HandleAsync(
					request
					, __timerManager
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
		services.Add(new(typeof(global::MainCore.Commands.UI.DebugViewModel.GetEndpointAdressQuery.Handler), typeof(global::MainCore.Commands.UI.DebugViewModel.GetEndpointAdressQuery.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.DebugViewModel.GetEndpointAdressQuery.Query, string>), typeof(global::MainCore.Commands.UI.DebugViewModel.GetEndpointAdressQuery.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.UI.DebugViewModel.GetEndpointAdressQuery.HandleBehavior), typeof(global::MainCore.Commands.UI.DebugViewModel.GetEndpointAdressQuery.HandleBehavior), lifetime));
		return services;
	}
}
