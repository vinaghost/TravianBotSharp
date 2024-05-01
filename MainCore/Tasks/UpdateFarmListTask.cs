using MainCore.Commands.Misc;
using MainCore.Infrasturecture.Persistence;
using MainCore.Tasks.Base;
using Microsoft.EntityFrameworkCore;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class UpdateFarmListTask : FarmListTask
    {
        public UpdateFarmListTask(IChromeManager chromeManager, UnitOfCommand unitOfCommand, UnitOfRepository unitOfRepository, IMediator mediator, IDbContextFactory<AppDbContext> contextFactory, DelayClickCommand delayClickCommand, IFarmParser farmParser) : base(chromeManager, unitOfCommand, unitOfRepository, mediator, contextFactory, delayClickCommand, farmParser)
        {
        }

        protected override async Task<Result> Execute()
        {
            Result result;
            var chromeBrowser = _chromeManager.Get(AccountId);
            result = await ToFarmListPage(chromeBrowser, CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

        protected override void SetName()
        {
            _name = "Update farm lists";
        }
    }
}