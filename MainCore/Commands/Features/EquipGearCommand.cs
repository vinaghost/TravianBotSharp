using FluentResults;
using HtmlAgilityPack;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Parsers;
using MainCore.Repositories;
using MainCore.Services;
using MediatR;
using OpenQA.Selenium;

namespace MainCore.Commands.Features
{
    public class EquipGearCommand : ByAccountIdBase, IRequest<Result>
    {
        public EquipGearCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    public class EquipGearCommandHandler : IRequestHandler<EquipGearCommand, Result>
    {
        private readonly UnitOfCommand _unitOfCommand;
        private readonly UnitOfParser _unitOfParser;
        private readonly UnitOfRepository _unitOfRepository;
        private readonly IChromeManager _chromeManager;
        private readonly ILogService _logService;

        public EquipGearCommandHandler(UnitOfCommand unitOfCommand, IChromeManager chromeManager, UnitOfRepository unitOfRepository, UnitOfParser unitOfParser, ILogService logService)
        {
            _unitOfCommand = unitOfCommand;
            _chromeManager = chromeManager;
            _unitOfRepository = unitOfRepository;
            _unitOfParser = unitOfParser;
            _logService = logService;
        }

        public async Task<Result> Handle(EquipGearCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            Result result;
            result = await _unitOfCommand.ToHeroInventoryCommand.Handle(new(accountId), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _unitOfCommand.UpdateHeroItemsCommand.Handle(new(accountId), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            var logger = _logService.GetLogger(accountId);

            var chromeBrowser = _chromeManager.Get(accountId);
            var html = chromeBrowser.Html;

            var heroItems = _unitOfRepository.HeroItemRepository.Get(accountId);
            var helmet = GetBetterHelmet(heroItems, html);

            if (helmet != HeroItemEnums.None)
            {
                logger.Information("Found new helmet: {helmet}", helmet);
                result = await ClickItem(chromeBrowser, helmet, cancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

                result = await _unitOfCommand.DelayClickCommand.Handle(new(accountId), cancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            }
            var body = GetBetterBody(heroItems, html);

            if (body != HeroItemEnums.None)
            {
                logger.Information("Found new armor: {body}", body);
                result = await ClickItem(chromeBrowser, body, cancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

                result = await _unitOfCommand.DelayClickCommand.Handle(new(accountId), cancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            }
            var shoes = GetBetterShoes(heroItems, html);

            if (shoes != HeroItemEnums.None)
            {
                logger.Information("Found new boots: {boots}", shoes);
                result = await ClickItem(chromeBrowser, shoes, cancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

                result = await _unitOfCommand.DelayClickCommand.Handle(new(accountId), cancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            }
            var horse = GetBetterHorse(heroItems, html);
            if (horse != HeroItemEnums.None)
            {
                logger.Information("Found new horse: {horse}", horse);
                result = await ClickItem(chromeBrowser, horse, cancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

                result = await _unitOfCommand.DelayClickCommand.Handle(new(accountId), cancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            }

            return Result.Ok();
        }

        public async Task<Result> ClickItem(IChromeBrowser chromeBrowser, HeroItemEnums item, CancellationToken cancellationToken)
        {
            var doc = chromeBrowser.Html;
            var node = _unitOfParser.HeroParser.GetItemSlot(doc, item);
            if (node is null)
            {
                return Result.Fail($"Cannot find item {item}");
            }

            var result = await chromeBrowser.Click(By.XPath(node.XPath));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            static bool inventoryLoaded(IWebDriver driver)
            {
                var html = new HtmlDocument();
                html.LoadHtml(driver.PageSource);
                var inventoryPageWrapper = html.DocumentNode.Descendants("div").FirstOrDefault(x => x.HasClass("inventoryPageWrapper"));
                return !inventoryPageWrapper.HasClass("loading");
            }

            result = await chromeBrowser.Wait(inventoryLoaded, cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            return Result.Ok();
        }

        private static HeroItemEnums GetBetterGear(HeroItemEnums currentGear, IList<HeroItemEnums> items, List<HeroItemEnums> listGear)
        {
            var inventoryGear = items.Intersect(listGear).ToList();
            if (!inventoryGear.Any()) return 0;

            var indexCurrentGear = listGear.IndexOf(currentGear);
            if (indexCurrentGear != -1)
            {
                //remove all item has lower attribute
                listGear.RemoveRange(indexCurrentGear, listGear.Count - indexCurrentGear);
            }

            //remove all item isn't in inventory
            listGear.RemoveAll(x => !inventoryGear.Contains(x));
            if (!listGear.Any()) return 0;

            return listGear.First();
        }

        private HeroItemEnums GetBetterHelmet(IList<HeroItemEnums> items, HtmlDocument doc)
        {
            var currentGear = (HeroItemEnums)_unitOfParser.HeroParser.GetHelmet(doc);
            var listGear = new List<HeroItemEnums>()
            {
                HeroItemEnums.HelmetExperience3,
                HeroItemEnums.HelmetRegeneration3,
                HeroItemEnums.HelmetExperience2,
                HeroItemEnums.HelmetRegeneration2,
                HeroItemEnums.HelmetExperience1,
                HeroItemEnums.HelmetRegeneration1,
            };

            return GetBetterGear(currentGear, items, listGear);
        }

        private HeroItemEnums GetBetterBody(IList<HeroItemEnums> items, HtmlDocument doc)
        {
            var currentGear = (HeroItemEnums)_unitOfParser.HeroParser.GetBody(doc);
            var listGear = new List<HeroItemEnums>()
            {
                HeroItemEnums.ArmorBreastplate3,
                HeroItemEnums.ArmorSegmented3,
                HeroItemEnums.ArmorScale3,
                HeroItemEnums.ArmorRegeneration3,
                HeroItemEnums.ArmorBreastplate2,
                HeroItemEnums.ArmorSegmented2,
                HeroItemEnums.ArmorScale2,
                HeroItemEnums.ArmorRegeneration2,
                HeroItemEnums.ArmorBreastplate1,
                HeroItemEnums.ArmorSegmented1,
                HeroItemEnums.ArmorScale1,
                HeroItemEnums.ArmorRegeneration1,
            };

            return GetBetterGear(currentGear, items, listGear);
        }

        private HeroItemEnums GetBetterShoes(IList<HeroItemEnums> items, HtmlDocument doc)
        {
            var currentGear = (HeroItemEnums)_unitOfParser.HeroParser.GetShoes(doc);
            var listGear = new List<HeroItemEnums>()
            {
                HeroItemEnums.BootsSpurs3,
                HeroItemEnums.BootsMercenery3,
                HeroItemEnums.BootsRegeneration3,
                HeroItemEnums.BootsSpurs2,
                HeroItemEnums.BootsMercenery2,
                HeroItemEnums.BootsRegeneration2,
                HeroItemEnums.BootsSpurs1,
                HeroItemEnums.BootsMercenery1,
                HeroItemEnums.BootsRegeneration1,
            };

            return GetBetterGear(currentGear, items, listGear);
        }

        private HeroItemEnums GetBetterHorse(IList<HeroItemEnums> items, HtmlDocument doc)
        {
            var currentGear = (HeroItemEnums)_unitOfParser.HeroParser.GetHorse(doc);
            var listGear = new List<HeroItemEnums>()
            {
                HeroItemEnums.Horse3,
                HeroItemEnums.Horse2,
                HeroItemEnums.Horse1,
            };

            return GetBetterGear(currentGear, items, listGear);
        }
    }
}