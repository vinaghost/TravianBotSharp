﻿using HtmlAgilityPack;
using OpenQA.Selenium.Chrome;
using System.Linq;
using System.Threading.Tasks;
using TravBotSharp.Files.Helpers;
using TravBotSharp.Files.Models.AccModels;

namespace TravBotSharp.Files.Tasks.LowLevel
{
    public class TTWarsBuyAdventure : BotTask
    {
        public override async Task<TaskRes> Execute(Account acc)
        {
            var htmlDoc = acc.Wb.Html;
            var wb = acc.Wb.Driver;
            await acc.Wb.Navigate($"{acc.AccInfo.ServerUrl}/hero.php?t=3");

            var button = htmlDoc.DocumentNode.Descendants("button").FirstOrDefault(x => x.HasClass("buyAdventure"));
            if (button == null)
            {
                this.ErrorMessage = "No button 'Buy' button found, perhaps you are not on vip ttwars server?";
                return TaskRes.Executed;
            }
            wb.ExecuteScript($"document.getElementById('{button.Id}').click()"); //Excgabge resources button
            return TaskRes.Executed;
        }
    }
}
