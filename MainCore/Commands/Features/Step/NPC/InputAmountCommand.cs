using FluentResults;
using MainCore.Commands.Base;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Parsers;
using MainCore.Repositories;
using MainCore.Services;
using OpenQA.Selenium;

namespace MainCore.Commands.Features.Step.NPC
{
    public class InputAmountCommand : ByAccountVillageIdBase, ICommand
    {
        public InputAmountCommand(AccountId accountId, VillageId villageId) : base(accountId, villageId)
        {
        }
    }

    [RegisterAsTransient]
    public class InputAmountCommandHandler : ICommandHandler<InputAmountCommand>
    {
        private readonly IUnitOfRepository _unitOfRepository;
        private readonly IChromeManager _chromeManager;
        private readonly IUnitOfParser _unitOfParser;

        public InputAmountCommandHandler(IUnitOfRepository unitOfRepository, IChromeManager chromeManager, IUnitOfParser unitOfParser)
        {
            _unitOfRepository = unitOfRepository;
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        public async Task<Result> Handle(InputAmountCommand command, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;

            var sum = _unitOfParser.MarketParser.GetSum(html);
            var ratio = GetRatio(command.VillageId);
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