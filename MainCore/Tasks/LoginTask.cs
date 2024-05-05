using MainCore.Commands.Features;
using MainCore.Commands.Features.DisableContextualHelp;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public sealed class LoginTask : AccountTask
    {
        public LoginTask(IMediator mediator) : base(mediator)
        {
        }

        protected override async Task<Result> Execute()
        {
            Result result;
            result = await new LoginCommand().Execute(_chromeBrowser, AccountId, CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await DisableContextualHelp();
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

        private async Task<Result> DisableContextualHelp()
        {
            var contextualHelpEnable = IsContextualHelpEnable();
            if (!contextualHelpEnable) return Result.Ok();

            Result result;
            result = await new ToOptionsPageCommand().Execute(_chromeBrowser, CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            result = await new DisableContextualHelpCommand().Execute(_chromeBrowser, CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await new ToDorfCommand().Execute(_chromeBrowser, 1, false, CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

        private bool IsContextualHelpEnable()
        {
            var html = _chromeBrowser.Html;
            var node = html.GetElementbyId("contextualHelp");
            return node is not null;
        }

        protected override void SetName()
        {
            _name = "Login task";
        }
    }
}