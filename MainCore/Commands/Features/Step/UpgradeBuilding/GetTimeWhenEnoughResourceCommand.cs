using MainCore.Common.Models;

namespace MainCore.Commands.Features.Step.UpgradeBuilding
{
    public class GetTimeWhenEnoughResourceCommand : ByAccountVillageIdBase, ICommand<TimeSpan>
    {
        public NormalBuildPlan Plan { get; }

        public GetTimeWhenEnoughResourceCommand(AccountId accountId, VillageId villageId, NormalBuildPlan plan) : base(accountId, villageId)
        {
            Plan = plan;
        }
    }

    [RegisterAsTransient]
    public class GetTimeWhenEnoughResourceCommandHandler : ICommandHandler<GetTimeWhenEnoughResourceCommand, TimeSpan>
    {
        private readonly IChromeManager _chromeManager;
        private readonly UnitOfParser _unitOfParser;
        private readonly UnitOfRepository _unitOfRepository;
        public TimeSpan Value { get; private set; }

        public GetTimeWhenEnoughResourceCommandHandler(IChromeManager chromeManager, UnitOfParser unitOfParser, UnitOfRepository unitOfRepository)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
            _unitOfRepository = unitOfRepository;
        }

        public async Task<Result> Handle(GetTimeWhenEnoughResourceCommand command, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;
            var isEmptySite = _unitOfRepository.BuildingRepository.EmptySite(command.VillageId, command.Plan.Location);
            Value = _unitOfParser.UpgradeBuildingParser.GetTimeWhenEnoughResource(html, isEmptySite, command.Plan.Type);
            return Result.Ok();
        }
    }
}