using FluentResults;
using MainCore.Commands.Base;
using MainCore.Common.MediatR;
using MainCore.Common.Models;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Parsers;
using MainCore.Repositories;
using MainCore.Services;

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
        private readonly IUnitOfParser _unitOfParser;
        private readonly UnitOfRepository _unitOfRepository;
        public TimeSpan Value { get; private set; }

        public GetTimeWhenEnoughResourceCommandHandler(IChromeManager chromeManager, IUnitOfParser unitOfParser, UnitOfRepository unitOfRepository)
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