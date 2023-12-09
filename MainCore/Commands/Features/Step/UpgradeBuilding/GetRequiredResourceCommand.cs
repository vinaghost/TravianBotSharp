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
    public class GetRequiredResourceCommand : ByAccountVillageIdBase, ICommand<long[]>
    {
        public NormalBuildPlan Plan { get; }

        public GetRequiredResourceCommand(AccountId accountId, VillageId villageId, NormalBuildPlan plan) : base(accountId, villageId)
        {
            Plan = plan;
        }
    }

    [RegisterAsTransient]
    public class GetRequiredResourceCommandHandler : ICommandHandler<GetRequiredResourceCommand, long[]>
    {
        private readonly IChromeManager _chromeManager;
        private readonly IUnitOfParser _unitOfParser;
        private readonly UnitOfRepository _unitOfRepository;

        public GetRequiredResourceCommandHandler(IChromeManager chromeManager, IUnitOfParser unitOfParser, UnitOfRepository unitOfRepository)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
            _unitOfRepository = unitOfRepository;
        }

        public long[] Value { get; private set; }

        public async Task<Result> Handle(GetRequiredResourceCommand command, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;

            var isEmptySite = _unitOfRepository.BuildingRepository.EmptySite(command.VillageId, command.Plan.Location);
            Value = _unitOfParser.UpgradeBuildingParser.GetRequiredResource(html, isEmptySite, command.Plan.Type);

            return Result.Ok();
        }
    }
}