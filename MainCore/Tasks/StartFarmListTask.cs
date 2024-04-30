using MainCore.Commands.Features;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class StartFarmListTask : AccountTask
    {
        private readonly ITaskManager _taskManager;

        private readonly IChromeManager _chromeManager;
        private readonly UnitOfParser _unitOfParser;

        public StartFarmListTask(UnitOfCommand unitOfCommand, UnitOfRepository unitOfRepository, IMediator mediator, ITaskManager taskManager, IChromeManager chromeManager, UnitOfParser unitOfParser) : base(unitOfCommand, unitOfRepository, mediator)
        {
            _taskManager = taskManager;
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        protected override async Task<Result> Execute()
        {
            Result result;
            result = await _mediator.Send(new ToFarmListPageCommand(AccountId), CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var chromeBrowser = _chromeManager.Get(AccountId);
            var html = chromeBrowser.Html;

            var useStartAllButton = _unitOfRepository.AccountSettingRepository.GetBooleanByName(AccountId, AccountSettingEnums.UseStartAllButton);
            if (useStartAllButton)
            {
                var startAllButton = _unitOfParser.FarmParser.GetStartAllButton(html);
                if (startAllButton is null) return Retry.ButtonNotFound("Start all farms");

                result = await chromeBrowser.Click(By.XPath(startAllButton.XPath));
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }
            else
            {
                var farmLists = _unitOfRepository.FarmRepository.GetActive(AccountId);
                if (farmLists.Count == 0) return Result.Fail(Skip.NoActiveFarmlist);

                foreach (var farmList in farmLists)
                {
                    var startButton = _unitOfParser.FarmParser.GetStartButton(html, farmList);
                    if (startButton is null) return Retry.ButtonNotFound($"Start farm {farmList}");

                    result = await chromeBrowser.Click(By.XPath(startButton.XPath));
                    if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                    await _unitOfCommand.DelayClickCommand.Handle(new(AccountId), CancellationToken);
                }
            }
            await SetNextExecute();
            return Result.Ok();
        }

        private async Task SetNextExecute()
        {
            var seconds = _unitOfRepository.AccountSettingRepository.GetByName(AccountId, AccountSettingEnums.FarmIntervalMin, AccountSettingEnums.FarmIntervalMax);
            ExecuteAt = DateTime.Now.AddSeconds(seconds);
            await _taskManager.ReOrder(AccountId);
        }

        protected override void SetName()
        {
            _name = "Start farm list";
        }
    }
}