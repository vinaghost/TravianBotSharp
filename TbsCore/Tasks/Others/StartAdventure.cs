﻿using System;
using System.Linq;
using System.Threading.Tasks;
using TbsCore.Helpers;
using TbsCore.Models.AccModels;
using TbsCore.Parsers;

namespace TbsCore.Tasks.Others
{
    public class StartAdventure : BotTask
    {
        /// <summary>
        /// In case we want to only update adventures
        /// </summary>
        public bool UpdateOnly { get; set; }

        public override async Task<TaskRes> Execute(Account acc)
        {
            await NavigationHelper.ToHero(acc, NavigationHelper.HeroTab.Adventures);

            acc.Hero.Adventures = AdventureParser.GetAdventures(acc.Wb.Html, acc.AccInfo.ServerVersion);

            HeroHelper.UpdateHeroVillage(acc);

            if (acc.Hero.Adventures == null || acc.Hero.Adventures.Count == 0 || UpdateOnly) return TaskRes.Executed;

            var adventures = acc.Hero.Adventures
                .Where(x =>
                    x.Coordinates.CalculateDistance(acc, HeroHelper.GetHeroHomeVillage(acc).Coordinates) <= acc.Hero.Settings.MaxDistance
                )
                .ToList();

            if (adventures.Count == 0) return TaskRes.Executed;

            var adventure = adventures.FirstOrDefault(x => x.Difficulty == Classificator.DifficultyEnum.Normal);
            if (adventure == null) adventure = adventures.FirstOrDefault();

            acc.Hero.NextHeroSend = DateTime.Now.AddSeconds(adventure.DurationSeconds * 2);

            switch (acc.AccInfo.ServerVersion)
            {
                case Classificator.ServerVersionEnum.TTwars:
                    await acc.Wb.Navigate($"{acc.AccInfo.ServerUrl}/{adventure.Ref}");

                    var startButton = acc.Wb.Html.GetElementbyId("start");
                    if (startButton == null)
                    {
                        //Hero is probably out of the village.
                        this.NextExecute = DateTime.Now.AddMinutes(10);
                        return TaskRes.Executed;
                    }
                    await DriverHelper.ClickById(acc, "start");
                    break;

                case Classificator.ServerVersionEnum.T4_5:
                    string script = $"var div = document.getElementById('{adventure.AdventureId}');";
                    script += $"div.children[0].submit();";
                    await DriverHelper.ExecuteScript(acc, script);

                    // Check hero outgoing time
                    var outTime = HeroParser.GetHeroArrival(acc.Wb.Html);
                    // At least 1.5x longer (if hero has Large map)
                    acc.Hero.NextHeroSend = DateTime.Now + TimeSpan.FromTicks((long)(outTime.Ticks * 1.5));
                    break;
            }

            if (DateTime.Now.Millisecond % 2 == 0)
            {
                await NavigationHelper.ToDorf1(acc);
            }
            else
            {
                await NavigationHelper.ToDorf2(acc);
            }

            return TaskRes.Executed;
        }
    }
}