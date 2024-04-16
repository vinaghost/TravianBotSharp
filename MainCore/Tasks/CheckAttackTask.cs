using Discord;
using Discord.Webhook;
using FluentResults;
using Humanizer;
using MainCore.Commands;
using MainCore.Common.Errors;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Parsers;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.Tasks.Base;
using MediatR;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class CheckAttackTask : VillageTask
    {
        private readonly IRallypointParser _rallypointParser;
        private readonly IAlertService _alertService;
        private readonly IChromeManager _chromeManager;
        private readonly ITaskManager _taskManager;
        private readonly ILogService _logService;

        public CheckAttackTask(UnitOfCommand unitOfCommand, UnitOfRepository unitOfRepository, IMediator mediator, IRallypointParser rallypointParser, IAlertService alertService, IChromeManager chromeManager, ITaskManager taskManager, ILogService logService) : base(unitOfCommand, unitOfRepository, mediator)
        {
            _rallypointParser = rallypointParser;
            _alertService = alertService;
            _chromeManager = chromeManager;
            _taskManager = taskManager;
            _logService = logService;
        }

        protected override async Task<Result> Execute()
        {
            Result result;
            result = await _unitOfCommand.ToDorfCommand.Handle(new(AccountId, 2), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _unitOfCommand.ToBuildingCommand.Handle(new(AccountId, 39), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _unitOfCommand.SwitchTabCommand.Handle(new(AccountId, 1), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _unitOfCommand.DelayClickCommand.Handle(new(AccountId), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            await CheckAttacks();

            return Result.Ok();
        }

        private async Task CheckAttacks()
        {
            var chromeBrowser = _chromeManager.Get(AccountId);
            var html = chromeBrowser.Html;

            var attacks = _rallypointParser.GetIncomingAttacks(html);

            var isAlert = _alertService.Update(AccountId, attacks);

            if (!isAlert) return;

            await AlertDiscord();
            await DonateResource();
            await EvadeTroop();
            await SetNextExecute();
        }

        private async Task AlertDiscord()
        {
            var enable = _unitOfRepository.AccountSettingRepository.GetBooleanByName(AccountId, Common.Enums.AccountSettingEnums.EnableDiscordAlert);
            if (!enable) return;

            var account = _unitOfRepository.AccountRepository.Get(AccountId);

            var webhookUrl = _unitOfRepository.AccountInfoRepository.GetDiscordWebhookUrl(AccountId);
            using var client = new DiscordWebhookClient(webhookUrl);
            var attacks = _alertService.Get(AccountId);
            var embed = new EmbedBuilder
            {
                Title = $"Server: {account.Server}",
                Description = $"Username: {account.Username} got {attacks.Sum(x => x.WaveCount)} attack",
            };

            foreach (var attack in attacks)
            {
                var sec = attack.DelaySecond == 0 ? "" : $"(and {attack.DelaySecond} seconds)";
                var tag = attack.IsNew ? "[NEW]" : "";
                embed.AddField(new EmbedFieldBuilder()
                {
                    Name = $"[{attack.VillageName}] ({attack.X} | {attack.Y})",
                    Value = $"{tag} {attack.Type.Humanize()} {attack.WaveCount} waves at {attack.ArrivalTime:yyyy-MM-dd HH:mm:ss} {sec} (servertime)",
                });
            }

            await client.SendMessageAsync(text: "@here Attack alert", embeds: new[] { embed.Build() });
        }

        private async Task DonateResource()
        {
            var enable = _unitOfRepository.VillageSettingRepository.GetBooleanByName(VillageId, Common.Enums.VillageSettingEnums.EnableDonateResource);
            if (!enable) return;

            var attacks = _alertService.Get(AccountId);

            var firstAttacks = attacks.FirstOrDefault();

            var time = firstAttacks.LocalTime.AddMinutes(-2);

            if (_taskManager.IsExist<DonateResourceTask>(AccountId, VillageId))
            {
                var task = _taskManager.Get<DonateResourceTask>(AccountId, VillageId);

                if (task.ExecuteAt > time)
                {
                    task.ExecuteAt = time;
                    await _taskManager.ReOrder(AccountId);
                }
            }
            else
            {
                await _taskManager.Add<DonateResourceTask>(AccountId, VillageId, executeTime: time);
            }
        }

        private async Task EvadeTroop()
        {
            var enable = _unitOfRepository.VillageSettingRepository.GetBooleanByName(VillageId, Common.Enums.VillageSettingEnums.EnableEvadeTroop);
            if (!enable) return;

            var attacks = _alertService.Get(AccountId);

            var firstAttacks = attacks.FirstOrDefault();

            var time = firstAttacks.LocalTime.AddMinutes(-1);

            if (_taskManager.IsExist<EvadeTroopTask>(AccountId, VillageId))
            {
                var task = _taskManager.Get<EvadeTroopTask>(AccountId, VillageId);

                if (task.ExecuteAt > time)
                {
                    task.ExecuteAt = time;
                    await _taskManager.ReOrder(AccountId);
                }
            }
            else
            {
                await _taskManager.Add<EvadeTroopTask>(AccountId, VillageId, executeTime: time);
            }
        }

        private async Task SetNextExecute()
        {
            var attacks = _alertService.Get(AccountId);
            if (attacks.Count == 0) return;
            const int MIN = 60 * 10;
            const int MAX = 60 * 20;
            var sec = Random.Shared.Next(MIN, MAX);

            var logger = _logService.GetLogger(AccountId);
            logger.Information("Will recheck rallypoint after {mins} mins.", sec / 60);

            ExecuteAt = DateTime.Now.AddSeconds(sec);
            await _taskManager.ReOrder(AccountId);
        }

        protected override void SetName()
        {
            var village = _unitOfRepository.VillageRepository.GetVillageName(VillageId);
            _name = $"Check attack in {village}";
        }
    }
}