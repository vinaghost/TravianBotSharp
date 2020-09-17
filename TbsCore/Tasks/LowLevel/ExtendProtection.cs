﻿using HtmlAgilityPack;
using OpenQA.Selenium.Chrome;
using System.Linq;
using System.Threading.Tasks;
using TravBotSharp.Files.Models.AccModels;

namespace TravBotSharp.Files.Tasks.LowLevel
{
    /// <summary>
    /// Extend beginners protection
    /// </summary>
    public class ExtendProtection : BotTask
    {
        public override async Task<TaskRes> Execute(Account acc)
        {
            var htmlDoc = acc.Wb.Html;

            //Sitters cannot extend protection on TTWARS!
            var extendButton = htmlDoc.DocumentNode.Descendants("button").FirstOrDefault(x => x.GetAttributeValue("value", "") == "Extend");
            if (extendButton == null)
            {
                this.ErrorMessage = "Can not extend protection! Are you a sitter?";
                return TaskRes.Executed;
            }

            //class dialogButtonOk
            //type submit http://prntscr.com/ryunwj
            return TaskRes.Executed;

        }
    }
}
