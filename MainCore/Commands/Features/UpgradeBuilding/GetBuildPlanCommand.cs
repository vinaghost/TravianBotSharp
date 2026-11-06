using System.Text.Json;

namespace MainCore.Commands.Features.UpgradeBuilding
{
    [Handler]
    public sealed partial class GetBuildPlanCommand(
        IDbContextFactory<AppDbContext> contextFactory,
        ILogger logger,
        RxQueue rxQueue)
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId) : IAccountVillageCommand
        {
            public string Name => nameof(GetBuildPlanCommand).Humanize();
        }

        private async ValueTask<Result<NormalBuildPlan>> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            var (accountId, villageId) = command;

            while (true)
            {
                if (cancellationToken.IsCancellationRequested) return Cancel.Error;

                using var context = contextFactory.CreateDbContext();
                var (_, isFailed, job, errors) = context.GetJob(accountId, villageId);
                if (isFailed) return Result.Fail(errors);

                if (job.Type == JobTypeEnums.ResourceBuild)
                {
                    var resourceBuildPlan = JsonSerializer.Deserialize<ResourceBuildPlan>(job.Content)!;

                    logger.Information("Extracting {ResourceBuildPlan}", resourceBuildPlan);

                    var normalBuildPlan = context.ExtractResourceBuildPlan(villageId, resourceBuildPlan);
                    if (normalBuildPlan is null)
                    {
                        context.DeleteJobById(job.Id);
                    }
                    else
                    {
                        context.AddJob(villageId, normalBuildPlan, true);
                    }

                    rxQueue.Enqueue(new JobsModified(accountId, villageId));
                    continue;
                }

                var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content)!;

                var isComplete = context.IsPlanDone(villageId, plan);
                if (isComplete)
                {
                    context.DeleteJobById(job.Id);
                    rxQueue.Enqueue(new JobsModified(accountId, villageId));
                    continue;
                }
                return plan;
            }
        }
    }
}