namespace MainCore.Services.AiPlanning
{
    [RegisterSingleton<AiPlannerService>]
    public sealed class AiPlannerService(IDbContextFactory<AppDbContext> factory, CustomServiceScopeFactory scopes, TaskManager tasks, RxQueue notifications)
    {
        private readonly SemaphoreSlim _operation = new(1, 1);
        private readonly PlannerApiClient _api = new();
        private readonly GuideContextService _guides = new();
        public PlannerSettingsStore Settings { get; } = new();

        public Task<AiModel[]> GetModelsAsync(PlannerOptions options, string key, CancellationToken token) => _api.GetModelsAsync(options.Provider, key, options.FreeOnly, token);

        public async Task<PlannerPreview> AnalyzeAsync(AccountId accountId, VillageId villageId, PlannerOptions options, string key, IProgress<string>? progress, CancellationToken token)
        {
            options.Validate();
            if (string.IsNullOrWhiteSpace(key)) throw new InvalidOperationException("Enter an API key for the selected provider.");
            if (!await _operation.WaitAsync(0, token)) throw new InvalidOperationException("Another planner operation is running. Wait for it to finish.");
            try
            {
                var snapshot = await new PlannerSnapshotService(factory, scopes, tasks).RefreshAsync(accountId, villageId, token, progress);
                // Reject an unprojectable pending queue before spending tokens.
                PlannerValidator.Project(snapshot, options.ApplyMode);
                progress?.Report("Checking model availability and loading guide context…");
                var models = await _api.GetModelsAsync(options.Provider, key, options.FreeOnly, token);
                var model = models.SingleOrDefault(x => x.Id == options.Model)
                    ?? throw new InvalidOperationException("That model is unavailable under the selected settings. Refresh the model list.");
                var guides = await _guides.GetAsync(options, token);
                progress?.Report("Asking the model to prepare the building queue…");
                var response = await _api.GenerateAsync(options, key, model, BuildContext(snapshot, options, guides), token);
                var validation = PlannerValidator.Validate(snapshot, options, response.Proposal);
                return new(Guid.NewGuid().ToString("N"), snapshot, options, response, validation, guides);
            }
            finally { _operation.Release(); }
        }

        public async Task<int> ApplyAsync(PlannerPreview preview, CancellationToken token, IProgress<string>? progress = null)
        {
            if (!await _operation.WaitAsync(0, token)) throw new InvalidOperationException("Another planner operation is running.");
            try
            {
                var accountId = new AccountId(preview.Snapshot.AccountId);
                var villageId = new VillageId(preview.Snapshot.VillageId);
                var snapshots = new PlannerSnapshotService(factory, scopes, tasks);
                var current = await snapshots.RefreshAsync(accountId, villageId, token, progress);
                if (current.Revision != preview.Snapshot.Revision) throw new InvalidOperationException("The live village changed since this preview. Analyze again before applying.");
                token.ThrowIfCancellationRequested();
                snapshots.EnsurePaused(accountId);
                using var db = factory.CreateDbContext();
                var count = PlannerQueueWriter.Apply(db, preview);
                notifications.Enqueue(new JobsModified(accountId, villageId));
                notifications.Enqueue(new BuildPlanApplied(accountId, villageId, count,
                    preview.Options.Goal.ToString(), preview.Options.ApplyMode.ToString(), preview.PlanId));
                return count;
            }
            finally { _operation.Release(); }
        }

        public void Undo(string planId, AccountId accountId, VillageId villageId)
        {
            if (!_operation.Wait(0)) throw new InvalidOperationException("Another planner operation is running.");
            try
            {
                new PlannerSnapshotService(factory, scopes, tasks).EnsurePaused(accountId);
                using var db = factory.CreateDbContext();
                PlannerQueueWriter.Undo(db, planId, accountId, villageId);
                notifications.Enqueue(new JobsModified(accountId, villageId));
            }
            finally { _operation.Release(); }
        }

        public static string BuildContext(VillageSnapshot snapshot, PlannerOptions options, GuidePassage[] guides)
        {
            var rules = Enum.GetValues<BuildingEnums>().Where(PlannerValidator.Supported)
                .Where(type => PlannerValidator.Restriction(type, snapshot.Tribe, options) is null)
                .Select(type => new
                {
                    id = (int)type, name = type.ToString(), maxSupportedLevel = PlannerValidator.MaxLevel(type, options),
                    multipleAllowed = type.IsMultipleBuilding(), prerequisites = type.GetPrerequisiteBuildings().Select(p => new { buildingId = (int)p.Type, level = p.Level }),
                    costsByTargetLevel = Enumerable.Range(1, PlannerValidator.MaxLevel(type, options)).Select(level => type.GetCost(level).Take(4).ToArray())
                });
            return PlannerJson.Write(new
            {
                goal = options.Goal == PlannerGoal.NewVillage ? "Prepare this village to settle another village as soon as feasible" : options.Goal.ToString(),
                queueMode = options.ApplyMode.ToString(), options.MaxSteps, options.HorizonHours, options.ServerSpeed, options.IsCapital,
                additionalSituation = options.Situation,
                village = new { snapshot.CapturedAt, tribe = snapshot.Tribe.ToString(), snapshot.UnderAttack, snapshot.HasPlus, snapshot.RomanQueue,
                    snapshot.Buildings, snapshot.Construction, snapshot.PendingJobs, snapshot.Resources, snapshot.ProductionPerHour, snapshot.ExecutionSettings },
                projectedStart = PlannerValidator.Project(snapshot, options.ApplyMode).Values,
                unknowns = new[] { "culture points/current and required", "used expansion slots", "settlers and chiefs already trained", "research", "troop counts and training queues", "construction duration", "external resource shipments" },
                constraints = new[] { "No demolition, gold spending or attack actions", "New Palace construction unsupported; existing Palace upgrades allowed", "No artifact-dependent or unsupported buildings", "One level per step, no skipped levels", "Storage upgrades must precede upgrades that exceed capacity", "One administrative building per village", "Do not assume the settlement goal is complete just because a building reached level 10", "Resource costs are ordered wood, clay, iron, crop" },
                buildingRules = rules,
                references = guides
            });
        }
    }
}
