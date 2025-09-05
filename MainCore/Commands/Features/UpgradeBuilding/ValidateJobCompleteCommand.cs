using System.Text.Json;

namespace MainCore.Commands.Features.UpgradeBuilding
{
    [Handler]
    public static partial class ValidateJobCompleteCommand
    {
        public sealed record Command(VillageId VillageId, JobDto job) : IVillageCommand;

        private static async ValueTask<bool> HandleAsync(
            Command command,
            AppDbContext context,
            ILogger logger,
            CancellationToken cancellationToken
        )
        {
            await Task.CompletedTask;
            var (villageId, job) = command;
            if (job.Type == JobTypeEnums.ResourceBuild) return false;

            var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content)!;

            // Gerçek bina seviyesini kontrol ediyorum - bu asıl validation olmalı
            var villageBuilding = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Location == plan.Location)
                .Select(x => x.Level)
                .FirstOrDefault();

            // Queue building kontrolü
            var queueBuilding = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Location == plan.Location)
                .FirstOrDefault();

            // Detaylı logging
            var queueCompleteTime = queueBuilding?.CompleteTime.ToString("HH:mm:ss") ?? "None";
            logger.Information("ValidateJobComplete: {Plan} - BuildingLevel={BuildingLevel}, QueueLevel={QueueLevel}, QueueCompleteTime={CompleteTime}", 
                $"{plan.Type} at location {plan.Location} to level {plan.Level}",
                villageBuilding,
                queueBuilding?.Level ?? 0,
                queueCompleteTime);
            
            // Önce gerçek building level kontrolü
            if (villageBuilding >= plan.Level) 
            {
                logger.Information("Job complete: Building level {BuildingLevel} >= Target level {TargetLevel}", villageBuilding, plan.Level);
                return true;
            }

            // Eğer queue'da tamamlanan inşa varsa, bu da job complete sayılabilir
            // CRITICAL FIX: Queue level kontrolünü geri ekliyorum - construction başladıktan sonra job'un tekrar işlenmesini engellemek için
            // Sadece queue'da hedef seviyeye ulaşmış VE tamamlanmış inşa varsa job complete sayılır
            if (queueBuilding is not null && 
                queueBuilding.Level >= plan.Level && 
                queueBuilding.CompleteTime <= DateTime.Now)
            {
                logger.Information("Job complete: Queue building level {QueueLevel} >= Target level {TargetLevel} and CompleteTime passed at {CompleteTime}", 
                    queueBuilding.Level, plan.Level, queueBuilding.CompleteTime.ToString("HH:mm:ss"));
                return true;
            }
            
            // Queue building detay logging
            if (queueBuilding is not null)
            {
                logger.Information("Queue analysis: Level={QueueLevel} vs Target={TargetLevel}, CompleteTime={CompleteTime} vs Now={Now}, TimeCheck={TimeCheck}",
                    queueBuilding.Level, plan.Level, queueBuilding.CompleteTime.ToString("HH:mm:ss"), DateTime.Now.ToString("HH:mm:ss"), queueBuilding.CompleteTime <= DateTime.Now);
            }

            // Job henüz tamamlanmamış
            logger.Information("Job not complete: Building level {BuildingLevel} < Target level {TargetLevel}", villageBuilding, plan.Level);
            return false;
        }
    }
}