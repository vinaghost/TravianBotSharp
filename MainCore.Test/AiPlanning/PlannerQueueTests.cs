using System.Text.Json;
using MainCore.Entities;
using MainCore.Enums;
using MainCore.Infrasturecture.Persistence;
using MainCore.Models;
using MainCore.Services.AiPlanning;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MainCore.Test.AiPlanning
{
    public sealed class PlannerQueueTests : IDisposable
    {
        private readonly SqliteConnection _connection = new("Data Source=:memory:");
        private readonly AppDbContext _db;
        public PlannerQueueTests()
        {
            _connection.Open();
            _db = new(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);
            _db.Database.EnsureCreated();
            _db.Accounts.Add(new Account { Id = 1, Username = "test", Server = "https://example.invalid" });
            _db.Villages.Add(new Village { Id = 1, Name = "Test", AccountId = 1 });
            _db.AccountsInfo.Add(new AccountInfo { AccountId = 1, Tribe = TribeEnums.Teutons });
            _db.VillagesSetting.Add(new VillageSetting { VillageId = 1, Setting = VillageSettingEnums.Tribe, Value = (int)TribeEnums.Teutons });
            foreach (var building in PlannerFixture.Snapshot().Buildings)
                _db.Buildings.Add(new Building { VillageId = 1, Location = building.Slot, Type = building.Type, Level = building.Level });
            _db.Storages.Add(new Storage { VillageId = 1, Wood = 12000, Clay = 12000, Iron = 12000, Crop = 12000, Warehouse = 80000, Granary = 80000, FreeCrop = 100 });
            _db.SaveChanges();
        }

        private PlannerPreview Preview(QueueApplyMode mode = QueueApplyMode.Append)
        {
            var state = PlannerSnapshotService.Read(_db, new(1), new(1));
            var options = new PlannerOptions { ApplyMode = mode };
            var response = new AiPlanResponse(PlannerFixture.Proposal(PlannerFixture.Step()), "test-model", 10, 10);
            return new(Guid.NewGuid().ToString("N"), state, options, response, PlannerValidator.Validate(state, options, response.Proposal), []);
        }

        private void AddJob() { _db.Jobs.Add(new Job { VillageId = 1, Position = 0, Type = JobTypeEnums.NormalBuild, Content = JsonSerializer.Serialize(new NormalBuildPlan { Type = BuildingEnums.MainBuilding, Location = 26, Level = 6 }) }); _db.SaveChanges(); }

        [Fact]
        public void EmptyReplacementDoesNotEraseThePendingQueue()
        {
            AddJob();
            var plan = Preview(QueueApplyMode.ReplacePending);
            plan = plan with { Response = plan.Response with { Proposal = PlannerFixture.Proposal() } };
            Should.Throw<InvalidOperationException>(() => PlannerQueueWriter.Apply(_db, plan));
            _db.Jobs.Count().ShouldBe(1);
            _db.AiPlanReceipts.Count().ShouldBe(0);
        }

        [Theory]
        [InlineData(AiProvider.OpenRouter)]
        [InlineData(AiProvider.OpenAI)]
        public void ProviderOutputBecomesExecutableJobsOnlyAfterExplicitApply(AiProvider provider)
        {
            var proposal = PlannerJson.Write(PlannerFixture.Proposal(PlannerFixture.Step()));
            var json = provider == AiProvider.OpenRouter
                ? PlannerJson.Write(new { choices = new[] { new { finish_reason = "stop", message = new { content = proposal } } } })
                : PlannerJson.Write(new { status = "completed", output = new[] { new { content = new[] { new { type = "output_text", text = proposal } } } } });
            using var response = JsonDocument.Parse(json);
            var parsed = PlannerApiClient.ReadResponse(provider, "fixture-model", response.RootElement);
            var plan = Preview();
            plan = plan with { Response = parsed, Validation = PlannerValidator.Validate(plan.Snapshot, plan.Options, parsed.Proposal) };
            plan.Validation.CanApply.ShouldBeTrue();
            _db.Jobs.Count().ShouldBe(0);
            PlannerQueueWriter.Apply(_db, plan).ShouldBe(1);
            var job = JsonSerializer.Deserialize<NormalBuildPlan>(_db.Jobs.Single().Content)!;
            job.Location.ShouldBe(21);
            job.Type.ShouldBe(BuildingEnums.Residence);
            job.Level.ShouldBe(10);
        }

        [Fact]
        public void ExecutionSettingChangesInvalidateThePreview()
        {
            var plan = Preview();
            _db.VillagesSetting.Add(new VillageSetting { VillageId = 1, Setting = VillageSettingEnums.CompleteImmediately, Value = 1 });
            _db.SaveChanges();
            Should.Throw<InvalidOperationException>(() => PlannerQueueWriter.Apply(_db, plan));
            _db.Jobs.Count().ShouldBe(0);
        }

        [Fact]
        public void AppendsValidJobsAndRejectsDuplicateApply()
        {
            AddJob();
            var plan = Preview();
            PlannerQueueWriter.Apply(_db, plan).ShouldBe(1);
            _db.Jobs.Count().ShouldBe(2);
            var job = _db.Jobs.OrderBy(x => x.Position).Last();
            job.Position.ShouldBe(1);
            var parsed = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content)!;
            parsed.Type.ShouldBe(BuildingEnums.Residence);
            parsed.Location.ShouldBe(21);
            parsed.Level.ShouldBe(10);
            Should.Throw<InvalidOperationException>(() => PlannerQueueWriter.Apply(_db, plan));
            _db.Jobs.Count().ShouldBe(2);
        }

        [Fact]
        public void ReplacePreservesActiveConstructionAndUndoRestoresPendingJobs()
        {
            AddJob();
            _db.QueueBuildings.Add(new QueueBuilding { VillageId = 1, Location = 26, Type = BuildingEnums.MainBuilding, Level = 6, CompleteTime = DateTime.Now.AddMinutes(2) });
            _db.SaveChanges();
            var plan = Preview(QueueApplyMode.ReplacePending);
            PlannerQueueWriter.Apply(_db, plan);
            _db.Jobs.Count().ShouldBe(1);
            _db.QueueBuildings.Count().ShouldBe(1);
            PlannerQueueWriter.Undo(_db, plan.PlanId, new(1), new(1));
            JsonSerializer.Deserialize<NormalBuildPlan>(_db.Jobs.Single().Content)!.Type.ShouldBe(BuildingEnums.MainBuilding);
            _db.QueueBuildings.Count().ShouldBe(1);
            _db.AiPlanReceipts.Single().Undone.ShouldBeTrue();
        }

        [Fact]
        public void RejectsStaleQueueWithoutRemovingAnything()
        {
            var plan = Preview(QueueApplyMode.ReplacePending);
            AddJob();
            Should.Throw<InvalidOperationException>(() => PlannerQueueWriter.Apply(_db, plan));
            _db.Jobs.Count().ShouldBe(1);
            _db.AiPlanReceipts.Count().ShouldBe(0);
        }

        [Fact]
        public void InvalidReplacementDoesNotDeleteExistingJobs()
        {
            AddJob();
            var plan = Preview(QueueApplyMode.ReplacePending);
            plan = plan with { Response = plan.Response with { Proposal = PlannerFixture.Proposal(new PlanStep(999, 21, 10, "Invalid")) } };
            Should.Throw<InvalidOperationException>(() => PlannerQueueWriter.Apply(_db, plan));
            _db.Jobs.Count().ShouldBe(1);
        }

        [Fact]
        public void RejectsExpiredOrWrongAccountPreview()
        {
            var plan = Preview();
            Should.Throw<InvalidOperationException>(() => PlannerQueueWriter.Apply(_db, plan with { Snapshot = plan.Snapshot with { CapturedAt = DateTimeOffset.UtcNow.AddHours(-1) } }));
            Should.Throw<InvalidOperationException>(() => PlannerQueueWriter.Apply(_db, plan with { Snapshot = plan.Snapshot with { AccountId = 999 } }));
            _db.Jobs.Count().ShouldBe(0);
        }

        [Fact]
        public void IgnoresTimerRoundingButDetectsStructuralQueueChanges()
        {
            var construction = new QueueBuilding { VillageId = 1, Location = 26, Type = BuildingEnums.MainBuilding, Level = 6, CompleteTime = DateTime.Now.AddMinutes(2) };
            _db.QueueBuildings.Add(construction); _db.SaveChanges();
            var original = Preview().Snapshot.Revision;
            construction.CompleteTime = construction.CompleteTime.AddMilliseconds(500); _db.SaveChanges();
            Preview().Snapshot.Revision.ShouldBe(original);
            construction.Level = 7; _db.SaveChanges();
            Preview().Snapshot.Revision.ShouldNotBe(original);
        }

        [Fact]
        public void UndoRejectsExecutedOrEditedJobs()
        {
            var plan = Preview(); PlannerQueueWriter.Apply(_db, plan);
            _db.Jobs.Single().Position = 2; _db.SaveChanges();
            Should.Throw<InvalidOperationException>(() => PlannerQueueWriter.Undo(_db, plan.PlanId, new(1), new(1)));
            _db.Jobs.Count().ShouldBe(1);
        }

        [Fact]
        public void AddsReceiptTableToAnExistingDatabaseIdempotently()
        {
            _db.Database.ExecuteSqlRaw("DROP TABLE AiPlanReceipts");
            PlannerQueueWriter.EnsureSchema(_db);
            PlannerQueueWriter.EnsureSchema(_db);
            _db.AiPlanReceipts.Count().ShouldBe(0);
            _db.Buildings.Count().ShouldBe(40);
        }

        public void Dispose() { _db.Dispose(); _connection.Dispose(); }
    }
}
