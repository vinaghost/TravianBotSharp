using MainCore.Infrasturecture.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MainCore.Tasks.Base
{
    public abstract class FarmListTask : AccountTask
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        protected readonly DelayClickCommand _delayClickCommand;

        protected readonly IFarmParser _farmParser;

        protected FarmListTask(IMediator mediator, IDbContextFactory<AppDbContext> contextFactory, DelayClickCommand delayClickCommand, IFarmParser farmParser) : base(mediator)
        {
            _contextFactory = contextFactory;
            _delayClickCommand = delayClickCommand;
            _farmParser = farmParser;
        }

        protected async Task<Result> ToFarmListPage(IChromeBrowser chromeBrowser, CancellationToken cancellationToken)
        {
            Result result;
            result = await ToPage(chromeBrowser, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            await Update(chromeBrowser, cancellationToken);
            return Result.Ok();
        }

        private async Task<Result> ToPage(IChromeBrowser chromeBrowser, CancellationToken cancellationToken)
        {
            Result result;

            var rallypointVillageId = GetVillageHasRallypoint(AccountId);
            if (rallypointVillageId == VillageId.Empty) return Skip.NoRallypoint;

            result = await new SwitchVillageCommand().Execute(_chromeBrowser, rallypointVillageId, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await new ToDorfCommand().Execute(_chromeBrowser, 2, false, CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await _mediator.Send(new UpdateBuildingCommand(AccountId, rallypointVillageId), cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await _mediator.Send(new ToBuildingCommand(chromeBrowser, 39), cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await new SwitchTabCommand().Execute(chromeBrowser, 4, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            await _delayClickCommand.Execute(AccountId);
            return Result.Ok();
        }

        private async Task Update(IChromeBrowser chromeBrowser, CancellationToken cancellationToken)
        {
            var html = chromeBrowser.Html;
            var dtos = _farmParser.Get(html);
            SaveToDatabase(dtos);
            await _mediator.Publish(new FarmListUpdated(AccountId), cancellationToken);
        }

        private VillageId GetVillageHasRallypoint(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();

            var village = context.Villages
                .Where(x => x.AccountId == accountId.Value)
                .Include(x => x.Buildings.Where(x => x.Type == BuildingEnums.RallyPoint))
                .Where(x => x.Buildings.Count > 0)
                .OrderByDescending(x => x.IsActive)
                .Select(x => x.Id)
                .AsEnumerable()
                .Select(x => new VillageId(x))
                .FirstOrDefault();
            return village;
        }

        private void SaveToDatabase(IEnumerable<FarmDto> dtos)
        {
            using var context = _contextFactory.CreateDbContext();
            var farms = context.FarmLists
                .Where(x => x.AccountId == AccountId.Value)
                .ToList();

            var ids = dtos.Select(x => x.Id.Value).ToList();

            var farmDeleted = farms.Where(x => !ids.Contains(x.Id)).ToList();
            var farmInserted = dtos.Where(x => !farms.Any(v => v.Id == x.Id.Value)).ToList();
            var farmUpdated = farms.Where(x => ids.Contains(x.Id)).ToList();

            farmDeleted.ForEach(x => context.Remove(x));
            farmInserted.ForEach(x => context.Add(x.ToEntity(AccountId)));

            foreach (var farm in farmUpdated)
            {
                var dto = dtos.FirstOrDefault(x => x.Id.Value == farm.Id);
                dto.To(farm);
                context.Update(farm);
            }

            context.SaveChanges();
        }
    }
}