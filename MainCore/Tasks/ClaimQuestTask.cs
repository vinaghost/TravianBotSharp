using MainCore.Commands.Features.ClaimQuest;
using MainCore.Tasks.Base;
using Microsoft.Extensions.DependencyInjection;

namespace MainCore.Tasks
{
    [RegisterTransient(Registration = RegistrationStrategy.Self)]
    public class ClaimQuestTask : VillageTask
    {
        protected override async Task<Result> Execute(IServiceScope scoped, CancellationToken cancellationToken)
        {
            Result result;
            var toQuestPageCommand = scoped.ServiceProvider.GetRequiredService<ToQuestPageCommand>();
            result = await toQuestPageCommand.Execute(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var claimQuestCommand = scoped.ServiceProvider.GetRequiredService<ClaimQuestCommand>();
            result = await claimQuestCommand.Execute(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }

        protected override void SetName()
        {
            var village = new GetVillageName().Execute(VillageId);
            _name = $"Claim quest in {village}";
        }
    }
}