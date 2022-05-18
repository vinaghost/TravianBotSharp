﻿using System.Linq;
using System.Threading.Tasks;
using TbsCore.Helpers;
using TbsCore.Models.AccModels;

namespace TbsCore.Tasks.Others
{
    //since "extend automatically" doesn't work on TTWars, this task will automatically prolong plus account / +25% resource boost
    public class TTWarsPlusAndBoost : BotTask
    {
        public override async Task<TaskRes> Execute(Account acc)
        {
            var leftBar = acc.Wb.Html.GetElementbyId("sidebarBeforeContent");
            var button = leftBar.Descendants("button").FirstOrDefault(x => x.HasClass("gold"));
            if (button == null)
            {
                return TaskRes.Executed;
            }
            var buttonid = button.GetAttributeValue("id", "");
            await Task.Delay(AccountHelper.Delay(acc) / 3);

            acc.Wb.ExecuteScript($"document.getElementById('{buttonid}').click()"); //boost production

            return TaskRes.Executed;
        }
    }
}