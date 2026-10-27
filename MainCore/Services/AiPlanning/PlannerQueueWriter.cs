using System.Text.Json;

namespace MainCore.Services.AiPlanning
{
    public static class PlannerQueueWriter
    {
        // Existing installations use EnsureCreated, not EF migrations. Add only the new table.
        public static void EnsureSchema(AppDbContext db) => db.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS "AiPlanReceipts" (
              "PlanId" TEXT NOT NULL PRIMARY KEY, "AccountId" INTEGER NOT NULL, "VillageId" INTEGER NOT NULL,
              "CreatedUtc" TEXT NOT NULL, "Goal" TEXT NOT NULL, "BeforeJobs" TEXT NOT NULL,
              "AfterJobs" TEXT NOT NULL, "Description" TEXT NOT NULL, "Undone" INTEGER NOT NULL);
            """);

        public static int Apply(AppDbContext db, PlannerPreview preview)
        {
            if (DateTimeOffset.UtcNow - preview.Snapshot.CapturedAt > TimeSpan.FromMinutes(15))
                throw new InvalidOperationException("This preview is older than 15 minutes. Analyze the village again.");
            EnsureSchema(db);
            using var transaction = db.Database.BeginTransaction();
            if (db.AiPlanReceipts.Any(x => x.PlanId == preview.PlanId)) throw new InvalidOperationException("This plan has already been applied.");
            var accountId = new AccountId(preview.Snapshot.AccountId);
            var villageId = new VillageId(preview.Snapshot.VillageId);
            var current = PlannerSnapshotService.Read(db, accountId, villageId, preview.Snapshot.ProductionPerHour);
            if (current.Revision != preview.Snapshot.Revision) throw new InvalidOperationException("The village or pending queue changed after analysis. Analyze again before applying.");
            var validation = PlannerValidator.Validate(current, preview.Options, preview.Response.Proposal);
            if (!validation.CanApply) throw new InvalidOperationException("The plan has no valid upgrades to apply. Analyze again.");
            var previous = db.Jobs.Where(x => x.VillageId == villageId.Value).OrderBy(x => x.Position).ThenBy(x => x.Id).ToList();
            var before = PlannerJson.Write(previous.Select(x => new PlannerJob(x.Id, x.Position, x.Type, x.Content)).ToArray());
            var start = previous.Count == 0 ? 0 : previous.Max(x => x.Position) + 1;
            if (preview.Options.ApplyMode == QueueApplyMode.ReplacePending)
            {
                db.Jobs.RemoveRange(previous);
                start = 0;
            }
            foreach (var (step, index) in preview.Response.Proposal.Steps.Select((step, index) => (step, index)))
                db.Jobs.Add(new Job
                {
                    VillageId = villageId.Value, Position = start + index, Type = JobTypeEnums.NormalBuild,
                    // Keep the executor's existing PascalCase format.
                    Content = JsonSerializer.Serialize(new NormalBuildPlan { Location = step.Slot, Type = (BuildingEnums)step.BuildingId, Level = step.TargetLevel })
                });
            db.SaveChanges();
            var after = ReadJobs(db, villageId.Value);
            db.AiPlanReceipts.Add(new AiPlanReceipt
            {
                PlanId = preview.PlanId, AccountId = accountId.Value, VillageId = villageId.Value,
                CreatedUtc = DateTimeOffset.UtcNow.ToString("O"), Goal = preview.Options.Goal.ToString(), BeforeJobs = before,
                AfterJobs = PlannerJson.Write(after), Description = PlannerJson.Write(new { preview.Response.Model, preview.Response.Proposal })
            });
            db.SaveChanges();
            transaction.Commit();
            return validation.Steps.Length;
        }

        public static void Undo(AppDbContext db, string planId, AccountId accountId, VillageId villageId)
        {
            EnsureSchema(db);
            using var transaction = db.Database.BeginTransaction();
            var receipt = db.AiPlanReceipts.SingleOrDefault(x => x.PlanId == planId && x.AccountId == accountId.Value && x.VillageId == villageId.Value)
                ?? throw new InvalidOperationException("Applied plan not found.");
            if (receipt.Undone) throw new InvalidOperationException("This plan has already been undone.");
            if (PlannerJson.Write(ReadJobs(db, villageId.Value)) != receipt.AfterJobs)
                throw new InvalidOperationException("Jobs were executed or edited after applying. Undo is unavailable; edit the remaining queue manually.");
            var before = JsonSerializer.Deserialize<PlannerJob[]>(receipt.BeforeJobs, PlannerJson.Options) ?? [];
            db.Jobs.RemoveRange(db.Jobs.Where(x => x.VillageId == villageId.Value));
            db.SaveChanges();
            foreach (var job in before) db.Jobs.Add(new Job { VillageId = villageId.Value, Position = job.Position, Type = job.Type, Content = job.Content });
            receipt.Undone = true;
            db.SaveChanges();
            transaction.Commit();
        }

        private static PlannerJob[] ReadJobs(AppDbContext db, int villageId) => db.Jobs.AsNoTracking().Where(x => x.VillageId == villageId)
            .OrderBy(x => x.Position).ThenBy(x => x.Id).Select(x => new PlannerJob(x.Id, x.Position, x.Type, x.Content)).ToArray();
    }
}
