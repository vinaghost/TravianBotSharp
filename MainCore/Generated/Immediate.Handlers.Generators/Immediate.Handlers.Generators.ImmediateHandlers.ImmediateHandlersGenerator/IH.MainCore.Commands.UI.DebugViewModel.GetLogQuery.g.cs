using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591

namespace MainCore.Commands.UI.DebugViewModel;

partial class GetLogQuery
{
	public sealed partial class Handler : global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.DebugViewModel.GetLogQuery.Query, string>
	{
		private readonly global::MainCore.Commands.UI.DebugViewModel.GetLogQuery.HandleBehavior _handleBehavior;

		public Handler(
			global::MainCore.Commands.UI.DebugViewModel.GetLogQuery.HandleBehavior handleBehavior
		)
		{
			var handlerType = typeof(GetLogQuery);

			_handleBehavior = handleBehavior;

		}

		public async global::System.Threading.Tasks.ValueTask<string> HandleAsync(
			global::MainCore.Commands.UI.DebugViewModel.GetLogQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken = default
		)
		{
			return await _handleBehavior
				.HandleAsync(request, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class HandleBehavior : global::Immediate.Handlers.Shared.Behavior<global::MainCore.Commands.UI.DebugViewModel.GetLogQuery.Query, string>
	{
		private readonly global::MainCore.Services.LogSink _logSink;

		public HandleBehavior(
			global::MainCore.Services.LogSink logSink
		)
		{
			_logSink = logSink;
		}

		public override async global::System.Threading.Tasks.ValueTask<string> HandleAsync(
			global::MainCore.Commands.UI.DebugViewModel.GetLogQuery.Query request,
			global::System.Threading.CancellationToken cancellationToken
		)
		{
			return await global::MainCore.Commands.UI.DebugViewModel.GetLogQuery
				.HandleAsync(
					request
					, _logSink
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
		services.Add(new(typeof(global::MainCore.Commands.UI.DebugViewModel.GetLogQuery.Handler), typeof(global::MainCore.Commands.UI.DebugViewModel.GetLogQuery.Handler), lifetime));
		services.Add(new(typeof(global::Immediate.Handlers.Shared.IHandler<global::MainCore.Commands.UI.DebugViewModel.GetLogQuery.Query, string>), typeof(global::MainCore.Commands.UI.DebugViewModel.GetLogQuery.Handler), lifetime));
		services.Add(new(typeof(global::MainCore.Commands.UI.DebugViewModel.GetLogQuery.HandleBehavior), typeof(global::MainCore.Commands.UI.DebugViewModel.GetLogQuery.HandleBehavior), lifetime));
		return services;
	}
}
