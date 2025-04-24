using MainCore.UI.ViewModels.UserControls;

namespace MainCore.Commands.UI.Villages
{
    [RegisterSingleton<MoveJobCommand>]
    public class MoveJobCommand
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly JobUpdated.Handler _jobUpdated;

        public MoveJobCommand(IDbContextFactory<AppDbContext> contextFactory, JobUpdated.Handler jobUpdated)
        {
            _contextFactory = contextFactory;
            _jobUpdated = jobUpdated;
        }

        public async Task Execute(AccountId accountId, VillageId villageId, ListBoxItemViewModel jobs, MoveEnums move)
        {
            if (!jobs.IsSelected) return;

            var oldIndex = jobs.SelectedIndex;

            if (!IsValid(move, oldIndex, jobs.Count)) return;

            var newIndex = GetNewIndex(move, oldIndex, jobs.Count);
            var oldJob = jobs[oldIndex];

            switch (move)
            {
                case MoveEnums.Up:
                case MoveEnums.Down:
                    Move(new JobId(oldJob.Id), new JobId(jobs[newIndex].Id));
                    break;

                case MoveEnums.Top:
                    MoveTop(new JobId(oldJob.Id));
                    break;

                case MoveEnums.Bottom:
                    MoveBottom(new JobId(oldJob.Id));
                    break;
            }

            await _jobUpdated.HandleAsync(new(accountId, villageId));
            jobs.SelectedIndex = newIndex;
        }

        private static bool IsValid(MoveEnums move, int oldIndex, int count)
        {
            switch (move)
            {
                case MoveEnums.Top:
                    return oldIndex != 0;

                case MoveEnums.Up:
                    return oldIndex != 0;

                case MoveEnums.Down:
                    return oldIndex != count - 1;

                case MoveEnums.Bottom:
                    return oldIndex != count - 1;

                default:
                    return true;
            }
        }

        private static int GetNewIndex(MoveEnums move, int oldIndex, int count)
        {
            switch (move)
            {
                case MoveEnums.Top:
                    return 0;

                case MoveEnums.Up:
                    return oldIndex - 1;

                case MoveEnums.Down:
                    return oldIndex + 1;

                case MoveEnums.Bottom:
                    return count - 1;

                default:
                    return 0;
            }
        }

        private void Move(JobId oldJobId, JobId newJobId)
        {
            using var context = _contextFactory.CreateDbContext();

            var jobIds = new List<int>() { oldJobId.Value, newJobId.Value };

            var jobs = context.Jobs
                .Where(x => jobIds.Contains(x.Id))
                .ToList();

            if (jobs.Count != 2) return;

            (jobs[0].Position, jobs[1].Position) = (jobs[1].Position, jobs[0].Position);
            context.UpdateRange(jobs);
            context.SaveChanges();
        }

        private void MoveTop(JobId jobId)
        {
            using var context = _contextFactory.CreateDbContext();

            var job = context.Jobs
                .Where(x => x.Id == jobId.Value)
                .FirstOrDefault();

            if (job is null) return;

            var currentPosition = job.Position;

            context.Jobs
                .Where(x => x.VillageId == job.VillageId)
                .Where(x => x.Position < currentPosition)
                .ExecuteUpdate(x => x.SetProperty(y => y.Position, y => y.Position + 1));

            job.Position = 0;
            context.Update(job);
            context.SaveChanges();
        }

        private void MoveBottom(JobId jobId)
        {
            using var context = _contextFactory.CreateDbContext();

            var job = context.Jobs
                .Where(x => x.Id == jobId.Value)
                .FirstOrDefault();

            if (job is null) return;

            var currentPosition = job.Position;

            context.Jobs
                .Where(x => x.VillageId == job.VillageId)
                .Where(x => x.Position > currentPosition)
                .ExecuteUpdate(x => x.SetProperty(y => y.Position, y => y.Position - 1));

            job.Position = context.Jobs
                .Where(x => x.VillageId == job.VillageId)
                .Max(x => x.Position) + 1;

            context.Update(job);
            context.SaveChanges();
        }
    }
}