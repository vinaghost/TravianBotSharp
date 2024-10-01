using MainCore.Commands.Abstract;
using MainCore.Commands.Features.UseHeroItem;
using MainCore.Common.Errors.AutoBuilder;
using MainCore.Common.Errors.Storage;
using MainCore.Common.Models;

namespace MainCore.Commands.Features.UpgradeBuilding
{
    [RegisterScoped(Registration = RegistrationStrategy.Self)]
    public class HandleResourceCommand(DataService dataService, IMediator mediator, UpdateStorageCommand updateStorageCommand, UseHeroResourceCommand useHeroResourceCommand, IDbContextFactory<AppDbContext> contextFactory, ToHeroInventoryCommand toHeroInventoryCommand) : CommandBase<NormalBuildPlan>(dataService)
    {
        private readonly IMediator _mediator = mediator;
        private readonly UpdateStorageCommand _updateStorageCommand = updateStorageCommand;
        private readonly UseHeroResourceCommand _useHeroResourceCommand = useHeroResourceCommand;
        private readonly ToHeroInventoryCommand _toHeroInventoryCommand = toHeroInventoryCommand;
        private readonly IDbContextFactory<AppDbContext> _contextFactory = contextFactory;

        public override async Task<Result> Execute(CancellationToken cancellationToken)
        {
            var plan = Data;

            Result result;
            result = await _updateStorageCommand.Execute(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var requiredResource = GetRequiredResource(plan.Type);

            result = new IsResourceEnough(_contextFactory).Execute(_dataService.VillageId, requiredResource);
            if (!result.IsFailed)
            {
                return Result.Ok();
            }

            if (result.HasError<FreeCrop>(out var freeCropErrors))
            {
                foreach (var item in freeCropErrors)
                {
                    item.Log(_dataService.Logger);
                }
                await AddCropland(cancellationToken);
                return Continue.Error;
            }

            if (result.HasError<StorageLimit>(out var storageLimitErrors))
            {
                foreach (var item in storageLimitErrors)
                {
                    item.Log(_dataService.Logger);
                }
                return Stop.NotEnoughStorageCapacity;
            }

            if (!IsUseHeroResource())
            {
                if (result.HasError<Resource>(out var resourceErrors))
                {
                    foreach (var item in resourceErrors)
                    {
                        item.Log(_dataService.Logger);
                    }
                }

                var time = UpgradeParser.GetTimeWhenEnoughResource(_dataService.ChromeBrowser.Html, plan.Type);
                return WaitResource.Error(time);
            }

            _dataService.Logger.Information("Use hero resource to upgrade building");
            var missingResource = new GetMissingResource(_contextFactory).Execute(_dataService.VillageId, requiredResource);

            var url = _dataService.ChromeBrowser.CurrentUrl;

            result = await _toHeroInventoryCommand.Execute(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var heroResourceResult = await _useHeroResourceCommand.Execute(missingResource, cancellationToken);
            if (heroResourceResult.IsFailed)
            {
                if (heroResourceResult.HasError<Retry>()) return heroResourceResult.WithError(TraceMessage.Error(TraceMessage.Line()));

                if (heroResourceResult.HasError<Resource>(out var resourceErrors))
                {
                    foreach (var item in resourceErrors)
                    {
                        item.Log(_dataService.Logger);
                    }
                }

                result = await _dataService.ChromeBrowser.Navigate(url, cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                var time = UpgradeParser.GetTimeWhenEnoughResource(_dataService.ChromeBrowser.Html, plan.Type);
                return WaitResource.Error(time);
            }

            result = await _dataService.ChromeBrowser.Navigate(url, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }

        private long[] GetRequiredResource(BuildingEnums building)
        {
            var doc = _dataService.ChromeBrowser.Html;

            var resources = UpgradeParser.GetRequiredResource(doc, building);
            if (resources is null) return [0, 0, 0, 0, 0];

            var resourceBuilding = new long[5];
            for (var i = 0; i < 5; i++)
            {
                resourceBuilding[i] = resources[i].InnerText.ParseLong();
            }

            return resourceBuilding;
        }

        private async Task AddCropland(CancellationToken cancellationToken)
        {
            var cropland = GetCropland();

            var plan = new NormalBuildPlan()
            {
                Location = cropland.Location,
                Type = cropland.Type,
                Level = cropland.Level + 1,
            };

            new AddJobCommand(_contextFactory).ToTop(_dataService.VillageId, plan);
            var logger = _dataService.Logger;
            logger.Information($"Add cropland to top of the job queue");
            await _mediator.Publish(new JobUpdated(_dataService.AccountId, _dataService.VillageId), cancellationToken);
        }

        private bool IsUseHeroResource()
        {
            var villageId = _dataService.VillageId;
            var useHeroResource = new GetSetting().BooleanByName(villageId, VillageSettingEnums.UseHeroResourceForBuilding);
            return useHeroResource;
        }

        private Building GetCropland()
        {
            var villageId = _dataService.VillageId;
            using var context = _contextFactory.CreateDbContext();
            var building = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type == BuildingEnums.Cropland)
                .OrderBy(x => x.Level)
                .FirstOrDefault();
            return building;
        }
    }
}