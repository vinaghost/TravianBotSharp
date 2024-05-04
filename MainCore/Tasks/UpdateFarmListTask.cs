using MainCore.Infrasturecture.Persistence;
using MainCore.Tasks.Base;
using Microsoft.EntityFrameworkCore;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class UpdateFarmListTask : FarmListTask
    {
        public UpdateFarmListTask(IMediator mediator, IDbContextFactory<AppDbContext> contextFactory, DelayClickCommand delayClickCommand, IFarmParser farmParser) : base(mediator, contextFactory, delayClickCommand, farmParser)
        {
        }

        protected override async Task<Result> Execute()
        {
            Result result;
            result = await ToFarmListPage(_chromeBrowser, CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

        protected override void SetName()
        {
            _name = "Update farm lists";
        }
    }
}