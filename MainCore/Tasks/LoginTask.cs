using MainCore.Commands.Features;
using MainCore.Commands.Features.DisableContextualHelp;
using MainCore.Tasks.Base;
using Microsoft.Extensions.DependencyInjection;

namespace MainCore.Tasks
{
    [RegisterTransient(Registration = RegistrationStrategy.Self)]
    public sealed class LoginTask : AccountTask
    {
        protected override async Task<Result> Execute(IServiceScope scoped, CancellationToken cancellationToken)
        {
            var loginCommand = scoped.ServiceProvider.GetRequiredService<LoginCommand>();
            Result result;
            result = await loginCommand.Execute(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await DisableContextualHelp(scoped, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

        private static async Task<Result> DisableContextualHelp(IServiceScope scoped, CancellationToken cancellationToken)
        {
            var dataService = scoped.ServiceProvider.GetRequiredService<DataService>();
            var chromeBrowser = dataService.ChromeBrowser;

            var contextualHelpEnable = OptionParser.IsContextualHelpEnable(chromeBrowser.Html);
            if (!contextualHelpEnable) return Result.Ok();

            Result result;
            var toOptionsPageCommand = scoped.ServiceProvider.GetRequiredService<ToOptionsPageCommand>();
            result = await toOptionsPageCommand.Execute(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var disableContextualHelpCommand = scoped.ServiceProvider.GetRequiredService<DisableContextualHelpCommand>();
            result = await disableContextualHelpCommand.Execute(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var toDorfCommand = scoped.ServiceProvider.GetRequiredService<ToDorfCommand>();
            result = await toDorfCommand.Execute(0, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

        protected override void SetName()
        {
            _name = "Login task";
        }
    }
}