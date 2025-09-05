using System.Text.Json;
using MainCore.Enums;
using MainCore.Errors;

namespace MainCore.Commands.Features.UpgradeBuilding
{
    [Handler]
    public static partial class GetBuildPlanCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId) : IAccountVillageCommand;

        private static async ValueTask<Result<NormalBuildPlan>> HandleAsync(
            Command command,
            GetJobCommand.Handler getJobQuery,
            ToDorfCommand.Handler toDorfCommand,
            UpdateBuildingCommand.Handler updateBuildingCommand,
            GetLayoutBuildingsCommand.Handler getLayoutBuildingsQuery,
            DeleteJobByIdCommand.Handler deleteJobByIdCommand,
            AddJobCommand.Handler addJobCommand,
            ValidateJobCompleteCommand.Handler validateJobCompleteCommand,
            AppDbContext context,
            ILogger logger,
            IRxQueue rxQueue,
            CancellationToken cancellationToken
        )
        {
            var (accountId, villageId) = command;

            // Infinite loop koruması - maksimum 10 deneme
            var attemptCount = 0;
            const int maxAttempts = 10;

            while (true)
            {
                if (cancellationToken.IsCancellationRequested) return Cancel.Error;
                
                // Çok fazla deneme yapılmasını engelle
                attemptCount++;
                if (attemptCount > maxAttempts)
                {
                    logger.Warning("GetBuildPlan reached maximum attempts ({MaxAttempts}) for village {VillageId}. This may indicate job validation issues.", maxAttempts, villageId);
                    return UpgradeBuildingError.BuildingJobQueueEmpty;
                }
                
                if (attemptCount > 1)
                {
                    logger.Information("GetBuildPlan attempt #{AttemptCount} for village {VillageId}", attemptCount, villageId);
                }

                var result = await toDorfCommand.HandleAsync(new(2), cancellationToken);
                if (result.IsFailed) return result;

                result = await updateBuildingCommand.HandleAsync(new(villageId), cancellationToken);
                if (result.IsFailed) return result;

                var (_, isFailed, job, errors) = await getJobQuery.HandleAsync(new(accountId, villageId), cancellationToken);
                if (isFailed) 
                {
                    // BuildingJobQueueEmpty ise direkt return et - yapacak iş yok
                    if (errors.Any(x => x is UpgradeBuildingError && x.Message.Contains("Building job queue is empty")))
                    {
                        logger.Information("GetJob failed: Building job queue is empty");
                        return UpgradeBuildingError.BuildingJobQueueEmpty;
                    }

                    // NextExecuteError varsa task'ı schedule et (construction queue full durumları için)
                    var nextExecuteErrors = errors.OfType<NextExecuteError>().ToList();
                    if (nextExecuteErrors.Count > 0)
                    {
                        logger.Information("GetJob failed with NextExecuteError, scheduling task for: {NextExecute}", 
                            nextExecuteErrors.First().NextExecute.ToString("HH:mm:ss"));
                    }
                    return Result.Fail(errors);
                }

                if (job.Type == JobTypeEnums.ResourceBuild)
                {
                    logger.Information("Processing ResourceBuild job: {Content}", job.Content);

                    var layoutBuildings = await getLayoutBuildingsQuery.HandleAsync(new(villageId, true));
                    var resourceBuildPlan = JsonSerializer.Deserialize<ResourceBuildPlan>(job.Content)!;
                    
                    // Plus account ve Romans tribe için birden fazla job üret
                    var plusActive = context.AccountsInfo
                        .Where(x => x.AccountId == accountId.Value)
                        .Select(x => x.HasPlusAccount)
                        .FirstOrDefault();
                    
                    var tribe = (TribeEnums)context.VillagesSetting
                        .Where(x => x.VillageId == villageId.Value)
                        .Where(x => x.Setting == VillageSettingEnums.Tribe)
                        .Select(x => x.Value)
                        .FirstOrDefault();
                    
                    var isRomans = tribe == TribeEnums.Romans;
                    
                    // İnşa kuyruğu kapasitesini hesapla
                    var maxJobs = 1; // Base: 1 slot
                    if (plusActive) maxJobs = 2; // Plus: +1 slot
                    if (isRomans) maxJobs = 3; // Romans: +1 slot (Plus ile birlikte 3)
                    
                    // TOTAL construction queue kontrolü - resource + infrastructure birlikte
                    var currentTotalQueueCount = context.QueueBuildings
                        .Where(x => x.VillageId == villageId.Value)
                        .Count(); // TÜM buildings (resource + infrastructure)
                    
                    var remainingSlots = maxJobs - currentTotalQueueCount;
                    logger.Information("ResourceBuild analysis: Plus={Plus}, Romans={Romans}, MaxJobs={MaxJobs}, CurrentTotalQueue={CurrentQueue}, RemainingSlots={RemainingSlots}", 
                        plusActive, isRomans, maxJobs, currentTotalQueueCount, remainingSlots);
                    
                    // Eğer kuyruk dolu ise task'ı schedule et
                    if (remainingSlots <= 0)
                    {
                        var nextCompleteTime = context.QueueBuildings
                            .Where(x => x.VillageId == villageId.Value)
                            .OrderBy(x => x.CompleteTime)
                            .Select(x => x.CompleteTime)
                            .FirstOrDefault();
                        
                        logger.Information("ResourceBuild queue full, scheduling task for: {NextExecute}", nextCompleteTime.ToString("HH:mm:ss"));
                        return NextExecuteError.ConstructionQueueFull(nextCompleteTime);
                    }
                    
                    // Resource field sayısını kontrol et - maksimum 2 resource aynı anda yapılabilir
                    var resourceTypes = new[] { BuildingEnums.Woodcutter, BuildingEnums.ClayPit, BuildingEnums.IronMine, BuildingEnums.Cropland };
                    var currentResourceQueueCount = context.QueueBuildings
                        .Where(x => x.VillageId == villageId.Value)
                        .Count(x => resourceTypes.Contains(x.Type));
                    
                    var maxResourceSlots = Math.Min(2, maxJobs); // Maksimum 2 resource field aynı anda
                    var remainingResourceSlots = maxResourceSlots - currentResourceQueueCount;
                    
                    logger.Information("Resource slot check: CurrentResourceQueue={Current}, MaxResourceSlots={Max}, RemainingResourceSlots={Remaining}", 
                        currentResourceQueueCount, maxResourceSlots, remainingResourceSlots);
                    
                    var addedJobs = 0;
                    for (int i = 0; i < Math.Min(remainingSlots, remainingResourceSlots); i++)
                    {
                        var normalBuildPlan = GetNormalBuildPlan(resourceBuildPlan, layoutBuildings);
                        if (normalBuildPlan is null) 
                        {
                            logger.Information("GetNormalBuildPlan returned null - no more upgradeable resources found");
                            break;
                        }
                        
                        // Resource validation - Job eklemeden önce resource kontrolü yap
                        var requiredResource = normalBuildPlan.Type.GetCost(normalBuildPlan.Level);
                        var storage = context.Storages
                            .Where(x => x.VillageId == villageId.Value)
                            .FirstOrDefault();
                        
                        if (storage != null && !HasEnoughResource(storage, requiredResource))
                        {
                            logger.Information("Skipping ResourceBuild job #{JobNumber}: {Plan} - Insufficient resources: Wood={Wood}/{ReqWood}, Clay={Clay}/{ReqClay}, Iron={Iron}/{ReqIron}, Crop={Crop}/{ReqCrop}", 
                                i + 1, $"{normalBuildPlan.Type} at location {normalBuildPlan.Location} to level {normalBuildPlan.Level}",
                                storage.Wood, requiredResource[0], storage.Clay, requiredResource[1], 
                                storage.Iron, requiredResource[2], storage.Crop, requiredResource[3]);
                            
                            // Hero resource kullanma ayarını kontrol et
                            var useHeroResource = context.BooleanByName(villageId, VillageSettingEnums.UseHeroResourceForBuilding);
                            if (useHeroResource)
                            {
                                logger.Information("UseHeroResourceForBuilding is enabled - job will be added and resources will be used from hero inventory during construction");
                                // Hero resource varsa job'u ekle, HandleResourceCommand'da hero resource kullanılacak
                                // Job eklemeye devam et
                            }
                            else
                            {
                                // Resource yetersizliği durumunda döngüden çık
                                // Timer parsing UpgradeBuildingTask'ta yapılacak
                                logger.Information("UseHeroResourceForBuilding is disabled - insufficient resources detected for {BuildingType} at location {Location} - will use Travian timer in UpgradeBuildingTask", 
                                    normalBuildPlan.Type, normalBuildPlan.Location);
                                break;
                            }
                        }
                        
                        logger.Information("Adding ResourceBuild job #{JobNumber}: {Plan}", i + 1, $"{normalBuildPlan.Type} at location {normalBuildPlan.Location} to level {normalBuildPlan.Level}");
                        await addJobCommand.HandleAsync(new(villageId, normalBuildPlan.ToJob(), true));
                        addedJobs++;
                        
                        // Bir sonraki job için building level'ı simüle et
                        var building = layoutBuildings.Find(x => x.Location == normalBuildPlan.Location);
                        if (building is not null)
                        {
                            building.JobLevel = Math.Max(building.JobLevel, normalBuildPlan.Level);
                        }
                    }
                    
                    // ResourceBuildPlan'ı sadece tüm resource'lar target level'a ulaştığında sil
                    // GetNormalBuildPlan null döndüyse ve hiç job eklenemiyorsa = tüm resources target level'da
                    var normalBuildPlanCheck = GetNormalBuildPlan(resourceBuildPlan, layoutBuildings);
                    if (normalBuildPlanCheck == null && addedJobs == 0)
                    {
                        logger.Information("All resources reached target level {Level}, deleting ResourceBuildPlan", resourceBuildPlan.Level);
                        await deleteJobByIdCommand.HandleAsync(new(job.Id), cancellationToken);
                    }
                    else if (addedJobs == 0)
                    {
                        logger.Information("ResourceBuild iteration skipped - no resource slots available ({CurrentResourceQueue}/{MaxResourceSlots}) or insufficient resources - keeping plan for next iteration", 
                            currentResourceQueueCount, maxResourceSlots);
                        
                        // Resource yetersizliği durumunda - kısa bekleme yap
                        // Gerçek timer parsing UpgradeBuildingTask'ta yapılacak
                        logger.Information("ResourceBuild iteration skipped due to insufficient resources - short wait to retry with actual timer parsing");
                        var nextExecuteTime = DateTime.Now.AddMinutes(2); // Kısa bekleme - gerçek timer UpgradeBuildingTask'ta parse edilecek
                        return NextExecuteError.MissingResource(nextExecuteTime);
                    }
                    else
                    {
                        logger.Information("ResourceBuild iteration completed: Added {AddedJobs} resource jobs from ResourceBuildPlan - keeping plan for next iteration", addedJobs);
                        // ResourceBuildPlan'ı koru - daha fazla upgrade gerekebilir
                    }
                    
                    rxQueue.Enqueue(new JobsModified(villageId));
                    continue;
                }

                var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content)!;

                var dorf = plan.Location < 19 ? 1 : 2;
                result = await toDorfCommand.HandleAsync(new(dorf), cancellationToken);
                if (result.IsFailed) return result;

                result = await updateBuildingCommand.HandleAsync(new(villageId), cancellationToken);
                if (result.IsFailed) return result;

                var isJobComplete = await validateJobCompleteCommand.HandleAsync(new ValidateJobCompleteCommand.Command(villageId, job), cancellationToken);
                logger.Information("Job validation result: {JobContent} - IsComplete={IsComplete}", job.Content, isJobComplete);
                
                if (isJobComplete)
                {
                    logger.Information("Deleting completed job: {JobId} - {JobContent}", job.Id, job.Content);
                    await deleteJobByIdCommand.HandleAsync(new(job.Id), cancellationToken);
                    rxQueue.Enqueue(new JobsModified(villageId));
                    continue;
                }

                // Individual job'lar için ek validation gerekmez - zaten queue'ya eklenirken validate edildi

                logger.Information("Job validation passed, proceeding with: {JobContent}", job.Content);

                // Job'u şimdilik silme - construction başarılı olduktan sonra silinecek
                // Bu sayede resource yetersizliği durumunda job korunacak
                logger.Information("Construction attempt will be made for: {JobContent}", job.Content);

                return plan;
            }
        }

        private static bool HasEnoughResource(Storage storage, long[] requiredResource)
        {
            return storage.Wood >= requiredResource[0] &&
                   storage.Clay >= requiredResource[1] &&
                   storage.Iron >= requiredResource[2] &&
                   storage.Crop >= requiredResource[3];
        }

        // CalculateMissingResources ve CalculateWaitTime fonksiyonları kaldırıldı
        // Artık Travian'ın kendi timer'ını kullanıyoruz - UpgradeParser.GetTimeWhenEnoughResource

        // GetFirstMissingResource fonksiyonu kaldırıldı - artık kullanılmıyor

        private static NormalBuildPlan? GetNormalBuildPlan(
            ResourceBuildPlan plan,
            List<BuildingItem> layoutBuildings
        )
        {
            List<BuildingItem> resourceFields;

            if (plan.Plan == ResourcePlanEnums.ExcludeCrop)
            {
                resourceFields = layoutBuildings
                    .Where(x => x.Type == BuildingEnums.Woodcutter || x.Type == BuildingEnums.ClayPit || x.Type == BuildingEnums.IronMine)
                    .Where(x => IsResourceUpgradeable(x, plan.Level))
                    .ToList();
            }
            else if (plan.Plan == ResourcePlanEnums.OnlyCrop)
            {
                resourceFields = layoutBuildings
                    .Where(x => x.Type == BuildingEnums.Cropland)
                    .Where(x => IsResourceUpgradeable(x, plan.Level))
                    .ToList();
            }
            else
            {
                resourceFields = layoutBuildings
                    .Where(x => x.Type.IsResourceField())
                    .Where(x => IsResourceUpgradeable(x, plan.Level))
                    .ToList();
            }

            if (resourceFields.Count == 0) return null;

            var minLevel = resourceFields
                .Select(x => x.Level)
                .Min();

            var chosenOne = resourceFields
                .Where(x => x.Level == minLevel)
                .OrderBy(x => x.Id.Value + Random.Shared.Next())
                .FirstOrDefault();

            if (chosenOne is null) return null;

            var normalBuildPlan = new NormalBuildPlan()
            {
                Type = chosenOne.Type,
                Level = chosenOne.Level + 1,
                Location = chosenOne.Location,
            };
            return normalBuildPlan;
        }

        // Resource upgrade edilebilirlik kontrol fonksiyonu
        private static bool IsResourceUpgradeable(BuildingItem building, int targetLevel)
        {
            // Eğer current level zaten hedef seviyeye ulaştıysa, upgrade edilemez
            if (building.CurrentLevel >= targetLevel) return false;

            // Eğer job level hedef seviyeyi geçtiyse, daha fazla upgrade planlanmış demektir
            if (building.JobLevel >= targetLevel) return false;

            // Queue'da bekleyen bir upgrade varsa, onun bitmesini bekle
            // Yeni job ekleme - zaten queue'da var
            if (building.QueueLevel > 0) return false;

            // Sadece current level'a göre kontrol et
            return building.CurrentLevel < targetLevel;
        }

        private static bool IsJobComplete(JobDto job, List<BuildingDto> buildings, List<QueueBuilding> queueBuildings)
        {
            if (job.Type == JobTypeEnums.ResourceBuild) return false;

            var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content)!;

            // Gerçek building level kontrolü - en güvenilir yöntem
            var villageBuilding = buildings
                .Where(x => x.Location == plan.Location)
                .Select(x => x.Level)
                .FirstOrDefault();
            if (villageBuilding >= plan.Level) return true;

            // Queue building kontrolü - construction başladıktan sonra job'un tekrar işlenmesini engellemek için
            var queueBuilding = queueBuildings
                .Where(x => x.Location == plan.Location)
                .OrderByDescending(x => x.Level)
                .FirstOrDefault();

            if (queueBuilding is not null && 
                queueBuilding.Level >= plan.Level && 
                queueBuilding.CompleteTime <= DateTime.Now)
            {
                return true;
            }

            return false;
        }
    }
}