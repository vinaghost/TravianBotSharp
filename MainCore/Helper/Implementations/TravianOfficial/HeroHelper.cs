﻿using FluentResults;
using HtmlAgilityPack;
using MainCore.Enums;
using MainCore.Helper.Interface;
using MainCore.Parser.Interface;
using MainCore.Services.Interface;
using OpenQA.Selenium;
using System.Linq;

namespace MainCore.Helper.Implementations.TravianOfficial
{
    public class HeroHelper : Base.HeroHelper
    {
        public HeroHelper(IChromeManager chromeManager, IHeroSectionParser heroSectionParser, INavigateHelper navigateHelper) : base(chromeManager, heroSectionParser, navigateHelper)
        {
        }

        public override Result ClickItem(int accountId, HeroItemEnums item)
        {
            var chromeBrowser = _chromeManager.Get(accountId);
            var doc = chromeBrowser.GetHtml();
            var node = _heroSectionParser.GetItemSlot(doc, (int)item);
            if (node is null)
            {
                return Result.Fail($"Cannot find item {item}");
            }

            var chrome = chromeBrowser.GetChrome();
            var elements = chrome.FindElements(By.XPath(node.XPath));
            if (elements.Count == 0)
            {
                return Result.Fail($"Cannot find item {item}");
            }

            elements[0].Click();
            var wait = chromeBrowser.GetWait();
            if (item.IsUsableWhenHeroAway())
            {
                wait.Until(driver =>
                {
                    var html = new HtmlDocument();
                    html.LoadHtml(driver.PageSource);
                    var dialog = _heroSectionParser.GetAmountBox(html);
                    return dialog is not null;
                });
            }
            else
            {
                wait.Until(driver =>
                {
                    var html = new HtmlDocument();
                    html.LoadHtml(driver.PageSource);
                    var inventoryPageWrapper = html.DocumentNode.Descendants("div").FirstOrDefault(x => x.HasClass("inventoryPageWrapper"));
                    return !inventoryPageWrapper.HasClass("loading");
                });
            }
            return Result.Ok();
        }

        public override Result Confirm(int accountId)
        {
            var chromeBrowser = _chromeManager.Get(accountId);
            var doc = chromeBrowser.GetHtml();
            var confirmButton = _heroSectionParser.GetConfirmButton(doc);
            if (confirmButton is null)
            {
                return Result.Fail("Cannot find confirm button");
            }
            var chrome = chromeBrowser.GetChrome();
            var elements = chrome.FindElements(By.XPath(confirmButton.XPath));
            if (elements.Count == 0)
            {
                return Result.Fail("Cannot find confirm button");
            }
            elements[0].Click();
            var wait = chromeBrowser.GetWait();

            wait.Until(driver =>
            {
                var html = new HtmlDocument();
                html.LoadHtml(driver.PageSource);
                var inventoryPageWrapper = html.DocumentNode.Descendants("div").FirstOrDefault(x => x.HasClass("inventoryPageWrapper"));
                return !inventoryPageWrapper.HasClass("loading");
            });

            return Result.Ok();
        }
    }
}