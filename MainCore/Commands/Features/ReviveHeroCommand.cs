using FluentResults;
using MainCore.Commands.Base;
using MainCore.Commands.Features.Step.UpgradeBuilding;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.Common.Errors.Storage;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Parsers;
using MainCore.Repositories;
using MainCore.Services;
using MediatR;
using OpenQA.Selenium;

namespace MainCore.Commands.Features
{
    public class ReviveHeroCommand : ByAccountIdBase, IRequest<Result>
    {
        public ReviveHeroCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    public class ReviveHeroCommandHandler : IRequestHandler<ReviveHeroCommand, Result>
    {
        private readonly UnitOfParser _unitOfParser;
        private readonly UnitOfCommand _unitOfCommand;
        private readonly UnitOfRepository _unitOfRepository;
        private readonly IChromeManager _chromeManager;
        private readonly ICommandHandler<UseHeroResourceCommand> _useHeroResourceCommand;

        public ReviveHeroCommandHandler(IChromeManager chromeManager, UnitOfParser unitOfParser, UnitOfCommand unitOfCommand, UnitOfRepository unitOfRepository, ICommandHandler<UseHeroResourceCommand> useHeroResourceCommand)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
            _unitOfCommand = unitOfCommand;
            _unitOfRepository = unitOfRepository;
            _useHeroResourceCommand = useHeroResourceCommand;
        }

        public async Task<Result> Handle(ReviveHeroCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            var chromeBrowser = _chromeManager.Get(accountId);

            var villageIdRaw = _unitOfRepository.AccountSettingRepository.GetByName(accountId, AccountSettingEnums.HeroRespawnVillage);
            var villageId = new VillageId(villageIdRaw);

            Result result;

            var currentUrl = chromeBrowser.CurrentUrl;

            result = await _unitOfCommand.ToDorfCommand.Handle(new(accountId, 1), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _unitOfCommand.SwitchVillageCommand.Handle(new(accountId, villageId), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await chromeBrowser.Navigate(currentUrl, cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            var html = chromeBrowser.Html;
            var requiredResource = _unitOfParser.HeroParser.GetRevivedResource(html);
            result = _unitOfRepository.StorageRepository.IsEnoughResource(villageId, requiredResource);
            if (result.IsFailed)
            {
                if (result.HasError<GranaryLimit>() || result.HasError<WarehouseLimit>())
                {
                    return result
                        .WithError(new Stop("Please check your storage"))
                        .WithError(new TraceMessage(TraceMessage.Line()));
                }
                var isUseHeroResource = _unitOfRepository.AccountSettingRepository.GetBooleanByName(accountId, AccountSettingEnums.UseHeroResourceToRevive);

                if (isUseHeroResource)
                {
                    var missingResource = _unitOfRepository.StorageRepository.GetMissingResource(villageId, requiredResource);
                    var heroResourceResult = await _useHeroResourceCommand.Handle(new(accountId, missingResource), cancellationToken);
                    if (heroResourceResult.IsFailed)
                    {
                        if (heroResourceResult.HasError<Retry>())
                        {
                            return heroResourceResult.WithError(new TraceMessage(TraceMessage.Line()));
                        }
                        else
                        {
                            return heroResourceResult
                                .WithError(new Skip("Not enough resource to revive hero"))
                                .WithError(new TraceMessage(TraceMessage.Line()));
                        }
                    }
                }
                else
                {
                    return result
                        .WithError(new Skip("Not enough resource to revive hero"))
                        .WithError(new TraceMessage(TraceMessage.Line()));
                }
            }

            html = chromeBrowser.Html;

            var reviveButton = _unitOfParser.HeroParser.GetReviveButton(html);
            if (reviveButton is null)
            {
                return Result.Fail(Retry.ButtonNotFound("revive"));
            }

            result = await chromeBrowser.Click(By.XPath(reviveButton.XPath));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}