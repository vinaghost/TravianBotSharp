using FluentResults;
using MainCore.Common.Enums;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Parsers;
using MainCore.Services;

namespace MainCore.Commands.Step.TrainTroop
{
    [RegisterAsTransient]
    public class GetMaximumTroopCommand : IGetMaximumTroopCommand
    {
        private readonly IChromeManager _chromeManager;
        private readonly IUnitOfParser _unitOfParser;

        public GetMaximumTroopCommand(IChromeManager chromeManager, IUnitOfParser unitOfParser)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        public int Value { get; private set; }

        public Result Execute(AccountId accountId, TroopEnums troop)
        {
            var chromeBrowser = _chromeManager.Get(accountId);
            var html = chromeBrowser.Html;
            Value = _unitOfParser.TroopPageParser.GetMaxAmount(html, troop);
            return Result.Ok();
        }
    }
}