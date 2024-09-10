using MainCore.Commands.Features.StartFarmList;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [RegisterTransient(Registration = RegistrationStrategy.Self)]
    public class UpdateFarmListTask : AccountTask
    {
        protected override async Task<Result> Execute()
        {
            Result result;
            result = await new ToFarmListPageCommand().Execute(_chromeBrowser, AccountId, CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            await new UpdateFarmlistCommand().Execute(_chromeBrowser, AccountId, CancellationToken);

            return Result.Ok();
        }

        protected override void SetName()
        {
            _name = "Update farm lists";
        }
    }
}