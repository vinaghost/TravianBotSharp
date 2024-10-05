using MainCore.Commands.Features.TrainTroop;
using MainCore.Common.Errors.TrainTroop;
using MainCore.Tasks.Base;
using Microsoft.Extensions.DependencyInjection;

namespace MainCore.Tasks
{
    [RegisterTransient(Registration = RegistrationStrategy.Self)]
    public class TrainTroopTask : VillageTask
    {
        private readonly ITaskManager _taskManager;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public TrainTroopTask(ITaskManager taskManager, IDbContextFactory<AppDbContext> contextFactory)
        {
            _taskManager = taskManager;
            _contextFactory = contextFactory;
        }

        protected override async Task<Result> Execute(IServiceScope scoped, CancellationToken cancellationToken)
        {
            var buildings = GetTrainTroopBuilding(VillageId);
            if (buildings.Count == 0) return Result.Ok();

            Result result;
            var settings = new Dictionary<VillageSettingEnums, int>();

            var trainTroopCommand = scoped.ServiceProvider.GetRequiredService<TrainTroopCommand>();
            foreach (var building in buildings)
            {
                result = await trainTroopCommand.Execute(building, cancellationToken);
                if (result.IsFailed)
                {
                    if (result.HasError<MissingBuilding>())
                    {
                        settings.Add(TrainTroopCommand.BuildingSettings[building], 0);
                    }
                    else if (result.HasError<MissingResource>())
                    {
                        break;
                    }
                }
            }

            Locator.Current.GetService<SetSettingCommand>().Execute(VillageId, settings);
            await SetNextExecute();
            return Result.Ok();
        }

        private async Task SetNextExecute()
        {
            var seconds = Locator.Current.GetService<GetSetting>().ByName(VillageId, VillageSettingEnums.TrainTroopRepeatTimeMin, VillageSettingEnums.TrainTroopRepeatTimeMax, 60);
            ExecuteAt = DateTime.Now.AddSeconds(seconds);
            await _taskManager.ReOrder(AccountId);
        }

        protected override string TaskName => "Train troop";

        private List<BuildingEnums> GetTrainTroopBuilding(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();
            var settings = new List<VillageSettingEnums>() {
                VillageSettingEnums.BarrackTroop,
                VillageSettingEnums.StableTroop,
                VillageSettingEnums.WorkshopTroop,
            };

            var filterdSettings = context.VillagesSetting
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => settings.Contains(x.Setting))
                .Where(x => x.Value != 0)
                .Select(x => x.Setting)
                .ToList();

            var buildings = new List<BuildingEnums>();

            if (filterdSettings.Contains(VillageSettingEnums.BarrackTroop))
            {
                buildings.Add(BuildingEnums.Barracks);
            }
            if (filterdSettings.Contains(VillageSettingEnums.StableTroop))
            {
                buildings.Add(BuildingEnums.Stable);
            }
            if (filterdSettings.Contains(VillageSettingEnums.WorkshopTroop))
            {
                buildings.Add(BuildingEnums.Workshop);
            }
            return buildings;
        }
    }
}