using MainCore.Constraints;
using MainCore.Models;

namespace MainCore.Commands.UI.Villages.AttackViewModel
{
    [Handler]
    public static partial class SendAttackCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId, AttackPlan Plan) : IAccountVillageCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            ToDorfCommand.Handler toDorfCommand,
            UpdateBuildingCommand.Handler updateBuildingCommand,
            ToBuildingCommand.Handler toBuildingCommand,
            SwitchTabCommand.Handler switchTabCommand,
            CancellationToken cancellationToken)
        {
            var (accountId, villageId, plan) = command;

            Result result;
            result = await toDorfCommand.HandleAsync(new(accountId, 2), cancellationToken);
            if (result.IsFailed) return result;

            var (_, isFailed, response, errors) = await updateBuildingCommand.HandleAsync(new(accountId, villageId), cancellationToken);
            if (isFailed) return Result.Fail(errors);

            var location = response.Buildings.FirstOrDefault(x => x.Type == BuildingEnums.RallyPoint)?.Location ?? 0;
            if (location == 0) return Skip.NoRallypoint;

            result = await toBuildingCommand.HandleAsync(new(accountId, location), cancellationToken);
            if (result.IsFailed) return result;

            // Open the send troops tab in the rally point
            // Tab index 2 corresponds to the "Send troops" tab
            result = await switchTabCommand.HandleAsync(new(accountId, 2), cancellationToken);
            if (result.IsFailed) return result;

            var script = BuildScript(plan);
            result = await browser.ExecuteJsScript(script);
            if (result.IsFailed) return result;

            return Result.Ok();
        }

        private static string BuildScript(AttackPlan plan)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"document.getElementsByName('x')[0].value='{plan.X}';");
            sb.AppendLine($"document.getElementsByName('y')[0].value='{plan.Y}';");
            sb.AppendLine($"var et=document.querySelector(\"input[name='eventType'][value='{(int)plan.Type}']\");if(et)et.checked=true;");
            for (int i = 0; i < plan.Troops.Length; i++)
            {
                var value = plan.Troops[i];
                if (value <= 0) continue;
                sb.AppendLine($"var input=document.getElementsByName('troop[t{i + 1}]')[0];if(input){{input.value='{value}';}}");
            }
            sb.AppendLine("document.querySelector('button[type=submit]').click();");
            return sb.ToString();
        }
    }
}
