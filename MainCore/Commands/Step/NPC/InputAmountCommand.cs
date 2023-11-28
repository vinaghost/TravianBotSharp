using FluentResults;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Parsers;
using MainCore.Repositories;
using MainCore.Services;
using OpenQA.Selenium;

namespace MainCore.Commands.Step.NPC
{
    [RegisterAsTransient]
    public class InputAmountCommand : IInputAmountCommand
    {
        private readonly IUnitOfRepository _unitOfRepository;
        private readonly IChromeManager _chromeManager;
        private readonly IUnitOfParser _unitOfParser;

        public InputAmountCommand(IUnitOfRepository unitOfRepository, IChromeManager chromeManager, IUnitOfParser unitOfParser)
        {
            _unitOfRepository = unitOfRepository;
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        public async Task<Result> Execute(AccountId accountId, VillageId villageId)
        {
            await Task.CompletedTask;
            var chromeBrowser = _chromeManager.Get(accountId);
            var html = chromeBrowser.Html;

            var sum = _unitOfParser.MarketParser.GetSum(html);
            var ratio = GetRatio(villageId);
            var sumRatio = ratio.Sum();
            var values = new long[4];
            for (var i = 0; i < 4; i++)
            {
                values[i] = sum * ratio[i] / sumRatio;
            }
            var sumValue = values.Sum();
            var diff = sumValue - sum;
            values[3] += diff;

            var inputs = _unitOfParser.MarketParser.GetInputs(html).ToArray();

            Result result;
            for (var i = 0; i < 4; i++)
            {
                result = await chromeBrowser.InputTextbox(By.XPath(inputs[i].XPath), $"{values[i]}");
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            }
            return Result.Ok();
        }

        private int[] GetRatio(VillageId villageId)
        {
            var settingNames = new List<VillageSettingEnums> {
                VillageSettingEnums.AutoNPCWood,
                VillageSettingEnums.AutoNPCClay,
                VillageSettingEnums.AutoNPCIron,
                VillageSettingEnums.AutoNPCCrop,
            };
            var settings = _unitOfRepository.VillageSettingRepository.GetByName(villageId, settingNames);

            var ratio = new int[4]
            {
                settings[VillageSettingEnums.AutoNPCWood],
                settings[VillageSettingEnums.AutoNPCClay],
                settings[VillageSettingEnums.AutoNPCIron],
                settings[VillageSettingEnums.AutoNPCCrop],
            };
            var sum = ratio.Sum();
            if (sum == 0)
            {
                ratio = Enumerable.Repeat(1, 4).ToArray();
            }

            return ratio;
        }
    }
}