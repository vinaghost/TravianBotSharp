using MainCore.Commands.Features.UpgradeBuilding;
using MainCore.Commands.Misc;
using MainCore.Tasks.Base;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Humanizer;

namespace MainCore.Tasks
{
    [Handler]
    public static partial class UpgradeBuildingTask
    {
        public sealed class Task : VillageTask
        {
            public Task(AccountId accountId, VillageId villageId) : base(accountId, villageId)
            {
            }

            protected override string TaskName => "Upgrade building";
        }

        private static async ValueTask<Result> HandleAsync(
            Task task,
            ILogger logger,
            IBrowser browser,
            GetBuildPlanCommand.Handler getBuildPlanCommand,
            ToBuildPageCommand.Handler toBuildPageCommand,
            HandleResourceCommand.Handler handleResourceCommand,
            AddCroplandCommand.Handler addCroplandCommand,
            HandleUpgradeCommand.Handler handleUpgradeCommand,
            UpdateBuildingCommand.Handler updateBuildingCommand,
            ICustomServiceScopeFactory serviceScopeFactory,
            CancellationToken cancellationToken)
        {
            logger.Information("=== TASK LIFECYCLE START === Task: {TaskName} | Village: {VillageId} | Account: {AccountId} | ExecuteAt: {ExecuteAt}", 
                "Upgrade building", task.VillageId, task.AccountId, task.ExecuteAt.ToString("HH:mm:ss"));
                
            Result result;
            var iterationCount = 0;

            while (true)
            {
                iterationCount++;
                logger.Information("=== ITERATION #{IterationCount} START === Getting build plan for village {VillageId}", iterationCount, task.VillageId);
                
                if (cancellationToken.IsCancellationRequested) 
                {
                    logger.Warning("=== TASK CANCELLED === Task: {TaskName} | Village: {VillageId} | Iteration: #{IterationCount}", "Upgrade building", task.VillageId, iterationCount);
                    return Cancel.Error;
                }

                NormalBuildPlan plan;
                try
                {
                    var (_, isFailed, planResult, errors) = await getBuildPlanCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
                    if (isFailed)
                {
                    // BuildingJobQueueEmpty ise task'ý bitir - yapacak iþ yok
                    if (errors.Any(x => x is UpgradeBuildingError && x.Message.Contains("Building job queue is empty")))
                    {
                        logger.Information("=== TASK COMPLETED === Task: {TaskName} | Village: {VillageId} | Reason: Building job queue is empty | Iteration: #{IterationCount}", 
                            "Upgrade building", task.VillageId, iterationCount);
                        return Result.Ok(); // Task'ý bitir
                    }

                    var nextExecuteErrors = errors.OfType<NextExecuteError>().OrderBy(x => x.NextExecute).ToList();
                    if (nextExecuteErrors.Count > 0)
                    {
                        // NextExecuteError'da MissingResource varsa Travian timer'ýný kullan
                        var missingResourceErrors = nextExecuteErrors.Where(x => x.Message.Contains("Missing resource for construction")).ToList();
                        if (missingResourceErrors.Count > 0)
                        {
                            // Ýlk eksik resource için sayfaya git ve timer parse et
                            try
                            {
                                
                                using var timerScope = serviceScopeFactory.CreateScope(task.AccountId);
                                var getLayoutBuildingsCommand = timerScope.ServiceProvider.GetRequiredService<GetLayoutBuildingsCommand.Handler>();
                                var layoutBuildings = await getLayoutBuildingsCommand.HandleAsync(new GetLayoutBuildingsCommand.Command(task.VillageId, false));
                                
                                var resourceBuildings = layoutBuildings
                                    .Where(x => x.Type == BuildingEnums.Woodcutter || 
                                                x.Type == BuildingEnums.ClayPit || 
                                                x.Type == BuildingEnums.IronMine || 
                                                x.Type == BuildingEnums.Cropland)
                                    .Where(x => x.Level > 0) // Sadece mevcut building'ler
                                    .Select(x => new { x.Type, x.Location, x.Level })
                                    .ToList();
                                
                                if (resourceBuildings.Count == 0)
                                {
                                    logger.Warning("No resource buildings found in layout for timer parsing");
                                    // Fallback: normal NextExecuteError zamanýný kullan
                                    task.ExecuteAt = nextExecuteErrors.Select(x => x.NextExecute).Min();
                                    return new Skip();
                                }
                                
                                TimeSpan shortestTime = TimeSpan.MaxValue;
                                DateTime earliestReadyTime = DateTime.MaxValue;
                                
                                // Her resource building için timer'ý kontrol et - GERÇEK LOCATION'LARI KULLAN
                                foreach (var building in resourceBuildings)
                                {
                                    try
                                    {
                                        logger.Information("Checking timer for {BuildingType} at location {Location}", building.Type, building.Location);
                                        result = await toBuildPageCommand.HandleAsync(new(task.VillageId, new NormalBuildPlan { Type = building.Type, Location = building.Location, Level = building.Level + 1 }), cancellationToken);
                                        if (!result.IsFailed)
                                        {
                                            var time = UpgradeParser.GetTimeWhenEnoughResource(browser.Html, building.Type);
                                            if (time > TimeSpan.Zero && time < shortestTime)
                                            {
                                                shortestTime = time;
                                                earliestReadyTime = DateTime.Now.Add(time);
                                                logger.Information("Found timer for {BuildingType} at location {Location}: {WaitTime}", building.Type, building.Location, time.Humanize());
                                            }
                                        }
                                        else
                                        {
                                            logger.Warning("Failed to navigate to {BuildingType} at location {Location}: {Error}", building.Type, building.Location, string.Join(", ", result.Reasons.Select(r => r.Message)));
                                        }
                                    }
                                    catch (Exception buildingEx)
                                    {
                                        logger.Warning("Timer parsing failed for {BuildingType} at location {Location}: {Error}", building.Type, building.Location, buildingEx.Message);
                                    }
                                }
                                
                                if (shortestTime < TimeSpan.MaxValue)
                                {
                                    task.ExecuteAt = earliestReadyTime;
                                    logger.Information("=== TASK SCHEDULED WITH TRAVIAN TIMER === Task: {TaskName} | Village: {VillageId} | NextExecute: {NextExecute} | WaitTime: {WaitTime} | Reason: MissingResource from GetBuildPlan | Iteration: #{IterationCount}", 
                                        "Upgrade building", task.VillageId, task.ExecuteAt.ToString("HH:mm:ss"), shortestTime.Humanize(), iterationCount);
                                    return new Skip();
                                }
                                else
                                {
                                    logger.Warning("No valid timer found for any resource building - all timer parsing failed");
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Warning("Timer parsing failed: {Error}", ex.Message);
                            }
                        }
                        
                        // Fallback: normal NextExecuteError zamanýný kullan
                        task.ExecuteAt = nextExecuteErrors.Select(x => x.NextExecute).Min();
                        logger.Information("=== TASK SCHEDULED === Task: {TaskName} | Village: {VillageId} | NextExecute: {NextExecute} | Reason: {Errors} | Iteration: #{IterationCount}", 
                            "Upgrade building", task.VillageId, task.ExecuteAt.ToString("HH:mm:ss"), string.Join(", ", errors.Select(e => e.Message)), iterationCount);
                    }
                    else
                    {
                        // Diðer hatalar için (timeout dahil) task'ý reschedule et
                        task.ExecuteAt = DateTime.Now.AddMinutes(2);
                        logger.Warning("=== TASK FAILED === Task: {TaskName} | Village: {VillageId} | Errors: {Errors} | NextExecute: {NextExecute} | Iteration: #{IterationCount}", 
                            "Upgrade building", task.VillageId, string.Join(", ", errors.Select(e => e.Message)), task.ExecuteAt.ToString("HH:mm:ss"), iterationCount);
                    }

                    return new Skip();
                }
                
                    plan = planResult;
                }
                catch (Exception ex)
                {
                    // GetBuildPlanCommand timeout durumunda task'ý reschedule et - silme!
                    task.ExecuteAt = DateTime.Now.AddMinutes(3);
                    logger.Warning("=== TASK RESCHEDULED === Task: {TaskName} | Village: {VillageId} | Reason: GetBuildPlanCommand exception | NextExecute: {NextExecute} | Error: {Error} | Iteration: #{IterationCount}", 
                        "Upgrade building", task.VillageId, task.ExecuteAt.ToString("HH:mm:ss"), ex.Message, iterationCount);
                    return new Skip();
                }

                logger.Information("=== CONSTRUCTION START === Task: {TaskName} | Village: {VillageId} | Building: {Type} level {Level} at location {Location} | Iteration: #{IterationCount}", 
                    "Upgrade building", task.VillageId, plan.Type, plan.Level, plan.Location, iterationCount);

                logger.Information("=== STEP 1/4 === Navigating to build page | Village: {VillageId} | Building: {Type} location {Location}", 
                    task.VillageId, plan.Type, plan.Location);
                    
                result = await toBuildPageCommand.HandleAsync(new(task.VillageId, plan), cancellationToken);
                if (result.IsFailed) 
                {
                    // Navigation hatasý durumunda task'ý reschedule et - silme!
                    task.ExecuteAt = DateTime.Now.AddMinutes(2);
                    logger.Warning("=== TASK RESCHEDULED === Task: {TaskName} | Village: {VillageId} | Reason: ToBuildPageCommand failed | NextExecute: {NextExecute} | Error: {Error} | Iteration: #{IterationCount}", 
                        "Upgrade building", task.VillageId, task.ExecuteAt.ToString("HH:mm:ss"), string.Join(", ", result.Reasons.Select(r => r.Message)), iterationCount);
                    return new Skip();
                }

                logger.Information("=== STEP 2/4 === Checking resources | Village: {VillageId} | Building: {Type} level {Level}", 
                    task.VillageId, plan.Type, plan.Level);
                    
                result = await handleResourceCommand.HandleAsync(new(task.AccountId, task.VillageId, plan), cancellationToken);
                if (result.IsFailed)
                {
                    if (result.HasError<LackOfFreeCrop>())
                    {
                        logger.Information("=== STEP 2/4 RETRY === Adding cropland | Village: {VillageId} | Reason: LackOfFreeCrop | Iteration: #{IterationCount}", 
                            task.VillageId, iterationCount);
                        await addCroplandCommand.HandleAsync(new(task.VillageId), cancellationToken);
                        continue;
                    }

                    if (result.HasError<StorageLimit>())
                    {
                        logger.Warning("=== TASK STOPPED === Task: {TaskName} | Village: {VillageId} | Reason: StorageLimit | Iteration: #{IterationCount}", 
                            "Upgrade building", task.VillageId, iterationCount);
                        return new Stop();
                    }
                    if (result.HasError<MissingResource>())
                    {
                        var time = UpgradeParser.GetTimeWhenEnoughResource(browser.Html, plan.Type);
                        task.ExecuteAt = DateTime.Now.Add(time);
                        logger.Information("=== TASK SCHEDULED === Task: {TaskName} | Village: {VillageId} | Reason: MissingResource | NextExecute: {NextExecute} | WaitTime: {WaitTime} | Iteration: #{IterationCount}", 
                            "Upgrade building", task.VillageId, task.ExecuteAt.ToString("HH:mm:ss"), time.Humanize(), iterationCount);
                        return new Skip();
                    }

                    // HandleResourceCommand'da beklenmeyen hata - reschedule et
                    task.ExecuteAt = DateTime.Now.AddMinutes(2);
                    logger.Warning("=== TASK RESCHEDULED === Task: {TaskName} | Village: {VillageId} | Reason: HandleResourceCommand failed | NextExecute: {NextExecute} | Error: {Error} | Iteration: #{IterationCount}", 
                        "Upgrade building", task.VillageId, task.ExecuteAt.ToString("HH:mm:ss"), string.Join(", ", result.Reasons.Select(r => r.Message)), iterationCount);
                    return new Skip();
                }

                logger.Information("=== STEP 3/4 === Starting construction | Village: {VillageId} | Building: {Type} level {Level} at location {Location}", 
                    task.VillageId, plan.Type, plan.Level, plan.Location);
                    
                result = await handleUpgradeCommand.HandleAsync(new(task.VillageId, plan), cancellationToken);
                if (result.IsFailed) 
                {
                    // Construction hatasý durumunda task'ý reschedule et - silme!
                    task.ExecuteAt = DateTime.Now.AddMinutes(2);
                    logger.Warning("=== TASK RESCHEDULED === Task: {TaskName} | Village: {VillageId} | Reason: HandleUpgradeCommand failed | NextExecute: {NextExecute} | Error: {Error} | Iteration: #{IterationCount}", 
                        "Upgrade building", task.VillageId, task.ExecuteAt.ToString("HH:mm:ss"), string.Join(", ", result.Reasons.Select(r => r.Message)), iterationCount);
                    return new Skip();
                }

                logger.Information("=== STEP 3/4 SUCCESS === Construction completed | Village: {VillageId} | Building: {Type} level {Level} at location {Location} | Iteration: #{IterationCount}", 
                    task.VillageId, plan.Type, plan.Level, plan.Location, iterationCount);
                
                // Construction baþarýlý olduktan sonra corresponding job'u sil
                // ?? CRITICAL: plan.Level = TARGET level (örn: 20), ama þu an sadece 1 level construction baþlattýk!
                using var scope = serviceScopeFactory.CreateScope(task.AccountId);
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                
                // Gerçek construction level'ý bul - queue'dan veya current building level + 1
                var currentBuilding = context.Buildings
                    .Where(x => x.VillageId == task.VillageId.Value)
                    .Where(x => x.Location == plan.Location)
                    .FirstOrDefault();
                
                var queueBuilding = context.QueueBuildings
                    .Where(x => x.VillageId == task.VillageId.Value)
                    .Where(x => x.Location == plan.Location)
                    .OrderByDescending(x => x.Level)
                    .FirstOrDefault();
                
                // Construction level = Queue'daki en yüksek level VEYA current level + 1
                var actualConstructionLevel = queueBuilding?.Level ?? (currentBuilding?.Level + 1 ?? plan.Level);
                
                logger.Information("=== CONSTRUCTION LEVEL DETECTION === Building: {Type} | Current: {CurrentLevel} | Queue: {QueueLevel} | Target: {TargetLevel} | Actual Construction: {ConstructionLevel}", 
                    plan.Type, currentBuilding?.Level ?? 0, queueBuilding?.Level ?? 0, plan.Level, actualConstructionLevel);
                
                // SADECE target level'a ulaþtýðýnda job'u sil!
                if (actualConstructionLevel >= plan.Level)
                {
                    logger.Information("=== TARGET REACHED === Target level {TargetLevel} reached with construction level {ConstructionLevel} - deleting job", 
                        plan.Level, actualConstructionLevel);
                        
                    var jobToDelete = context.Jobs
                        .Where(x => x.VillageId == task.VillageId.Value)
                        .Where(x => x.Type == JobTypeEnums.NormalBuild)
                        .Select(x => new { x.Id, Content = x.Content })
                        .AsEnumerable()
                        .Where(x => 
                        {
                            var jobPlan = JsonSerializer.Deserialize<NormalBuildPlan>(x.Content)!;
                            return jobPlan.Location == plan.Location && 
                                   jobPlan.Level == plan.Level &&  // ? TARGET level ile eþleþ (20)!
                                   jobPlan.Type == plan.Type;
                        })
                        .FirstOrDefault();
                    
                    if (jobToDelete is not null)
                    {
                        logger.Information("=== JOB COMPLETED & DELETED === Target reached | Village: {VillageId} | JobId: {JobId} | Building: {Plan} | Iteration: #{IterationCount}", 
                            task.VillageId, jobToDelete.Id, $"{plan.Type} at location {plan.Location} to level {plan.Level}", iterationCount);
                        var deleteJobByIdCommand = scope.ServiceProvider.GetRequiredService<DeleteJobByIdCommand.Handler>();
                        await deleteJobByIdCommand.HandleAsync(new(new JobId(jobToDelete.Id)), cancellationToken);
                        
                        var rxQueue = scope.ServiceProvider.GetRequiredService<IRxQueue>();
                        rxQueue.Enqueue(new JobsModified(task.VillageId));
                    }
                    else
                    {
                        logger.Warning("=== TARGET JOB NOT FOUND === No target job found for deletion | Village: {VillageId} | Building: {Type} level {Level} at location {Location} | Iteration: #{IterationCount}", 
                            task.VillageId, plan.Type, plan.Level, plan.Location, iterationCount);
                    }
                }
                else
                {
                    logger.Information("=== TARGET NOT REACHED === Current construction: {ConstructionLevel}, Target: {TargetLevel} - keeping job for next iteration", 
                        actualConstructionLevel, plan.Level);
                }

                logger.Information("=== STEP 4/4 === Updating building data | Village: {VillageId}", task.VillageId);
                
                result = await updateBuildingCommand.HandleAsync(new(task.VillageId), cancellationToken);
                if (result.IsFailed) 
                {
                    // UpdateBuilding hatasý durumunda task'ý reschedule et - silme!
                    task.ExecuteAt = DateTime.Now.AddMinutes(2);
                    logger.Warning("=== TASK RESCHEDULED === Task: {TaskName} | Village: {VillageId} | Reason: UpdateBuildingCommand failed | NextExecute: {NextExecute} | Error: {Error} | Iteration: #{IterationCount}", 
                        "Upgrade building", task.VillageId, task.ExecuteAt.ToString("HH:mm:ss"), string.Join(", ", result.Reasons.Select(r => r.Message)), iterationCount);
                    return new Skip();
                }
                
                logger.Information("=== ITERATION #{IterationCount} COMPLETED === All steps successful | Village: {VillageId} | Building: {Type} level {Level} at location {Location} | Continuing to next job...", 
                    iterationCount, task.VillageId, plan.Type, plan.Level, plan.Location);
            }
        }
    }
}
