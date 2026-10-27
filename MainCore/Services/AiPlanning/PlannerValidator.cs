using System.Text.Json;

namespace MainCore.Services.AiPlanning
{
    public static class PlannerValidator
    {
        private static readonly BuildingEnums[] Administration = [BuildingEnums.Residence, BuildingEnums.Palace, BuildingEnums.CommandCenter];
        // Standard warehouse/granary capacities. Live capacity is retained as the starting value.
        // https://knowledgebase.legends.travian.com/en-US/buildings/10 (and /11)
        private static readonly long[] Capacity = [0, 1200, 1700, 2300, 3100, 4000, 5000, 6300, 7800, 9600, 11800, 14400, 17600, 21400, 25900, 31300, 37900, 45700, 55100, 66400, 80000];

        public static bool Supported(BuildingEnums type) => Enum.IsDefined(type)
            && type is not (BuildingEnums.Unknown or BuildingEnums.Site or BuildingEnums.Blacksmith or BuildingEnums.WW or BuildingEnums.GreatWarehouse or BuildingEnums.GreatGranary)
            && type.GetCost(1).Take(4).Sum() > 0;

        public static int MaxLevel(BuildingEnums type, PlannerOptions options) => type.IsResourceField()
            ? options.IsCapital == true ? 20 : 10 : type.GetMaxLevel();

        public static string? Restriction(BuildingEnums type, TribeEnums tribe, PlannerOptions options)
        {
            if (type.IsWall() && type != tribe.GetWall()) return "This wall belongs to another tribe.";
            if (type == BuildingEnums.Trapper && tribe != TribeEnums.Gauls || type == BuildingEnums.Brewery && tribe != TribeEnums.Teutons
                || type == BuildingEnums.HorseDrinkingTrough && tribe != TribeEnums.Romans || type == BuildingEnums.CommandCenter && tribe != TribeEnums.Huns
                || type == BuildingEnums.Waterworks && tribe != TribeEnums.Egyptians) return "This building belongs to another tribe.";
            if (type is BuildingEnums.StonemasonsLodge or BuildingEnums.Brewery && options.IsCapital != true) return "Confirm capital status before planning this building.";
            if (type is BuildingEnums.GreatBarracks or BuildingEnums.GreatStable && options.IsCapital != false) return "This building requires a confirmed non-capital village.";
            return null;
        }

        public static PlanValidation Validate(VillageSnapshot snapshot, PlannerOptions options, AiProposal proposal)
        {
            options.Validate();
            var errors = new List<string>();
            var warnings = new List<string>();
            var rows = new List<PreviewStep>();
            var totals = new long[4];
            if (proposal.Assessment is null || proposal.FollowUpActions is null || proposal.Assumptions is null
                || proposal.Steps is null || proposal.FollowUpActions.Any(x => x is null) || proposal.Assumptions.Any(x => x is null))
                return new([], ["The model omitted required plan fields."], [], totals);
            if (proposal.Steps.Length > options.MaxSteps)
                return new([], [$"The model must return at most {options.MaxSteps} upgrades."], [], totals);
            if (proposal.Assessment.Length > 12000 || proposal.FollowUpActions.Length > 30 || proposal.Assumptions.Length > 30
                || proposal.FollowUpActions.Concat(proposal.Assumptions).Any(x => x.Length > 4000))
                return new([], ["The model returned excessive explanation text."], [], totals);
            Dictionary<int, PlannerBuilding> projected;
            try { projected = Project(snapshot, options.ApplyMode); }
            catch (InvalidOperationException ex) { return new([], [ex.Message], [], totals); }
            var warehouse = snapshot.Resources.Warehouse + StorageDelta(snapshot.Buildings, projected.Values, BuildingEnums.Warehouse);
            var granary = snapshot.Resources.Granary + StorageDelta(snapshot.Buildings, projected.Values, BuildingEnums.Granary);
            foreach (var step in proposal.Steps)
            {
                var prefix = $"Step {rows.Count + errors.Count + 1}";
                if (step is null || string.IsNullOrWhiteSpace(step.Reason) || step.Reason.Length > 2000)
                { errors.Add($"{prefix}: missing or excessive explanation."); continue; }
                var type = (BuildingEnums)step.BuildingId;
                if (!Supported(type)) { errors.Add($"{prefix}: unsupported building ID {step.BuildingId}."); continue; }
                if (!projected.TryGetValue(step.Slot, out var old)) { errors.Add($"{prefix}: slot {step.Slot} does not exist."); continue; }
                var level = old.Type == BuildingEnums.Site ? 0 : Math.Max(old.Level, 0);
                if (old.Type != BuildingEnums.Site && old.Type != type) { errors.Add($"{prefix}: slot {step.Slot} already contains {old.Type}."); continue; }
                if (step.TargetLevel != level + 1 || step.TargetLevel > MaxLevel(type, options))
                { errors.Add($"{prefix}: {type} must advance exactly one level, within the supported village limit."); continue; }
                var restriction = Restriction(type, snapshot.Tribe, options);
                if (restriction is not null) { errors.Add($"{prefix}: {restriction}"); continue; }
                if (type.IsResourceField() != (step.Slot <= 18)
                    || type.IsWall() != (step.Slot == 40) || type == BuildingEnums.RallyPoint != (step.Slot == 39)
                    || type == BuildingEnums.MainBuilding && step.Slot != 26)
                { errors.Add($"{prefix}: {type} cannot be placed in slot {step.Slot}."); continue; }
                if (level == 0)
                {
                    if (type == BuildingEnums.Palace) { errors.Add($"{prefix}: new palaces require an account-wide uniqueness check; build one manually first."); continue; }
                    if (!type.IsMultipleBuilding() && projected.Values.Any(x => x.Slot != step.Slot && x.Type == type))
                    { errors.Add($"{prefix}: a {type} already exists."); continue; }
                    if (type.IsMultipleBuilding() && projected.Values.Any(x => x.Type == type)
                        && !projected.Values.Any(x => x.Type == type && x.Level >= type.GetMaxLevel()))
                    { errors.Add($"{prefix}: finish an existing {type} before adding another."); continue; }
                    if (Administration.Contains(type) && projected.Values.Any(x => x.Slot != step.Slot && Administration.Contains(x.Type)))
                    { errors.Add($"{prefix}: Residence, Palace and Command Center are mutually exclusive."); continue; }
                }
                var missing = type.GetPrerequisiteBuildings().Where(p => !projected.Values.Any(b => b.Type == p.Type && b.Level >= p.Level)).ToArray();
                if (missing.Length > 0) { errors.Add($"{prefix}: first complete {string.Join(", ", missing.Select(p => $"{p.Type} {p.Level}"))}."); continue; }
                var cost = type.GetCost(step.TargetLevel).Take(4).ToArray();
                if (cost.Take(3).Any(x => x > warehouse) || cost[3] > granary)
                { errors.Add($"{prefix}: upgrade storage earlier; this upgrade costs more than the projected capacity."); continue; }
                if (type == BuildingEnums.Warehouse) warehouse += Capacity[step.TargetLevel] - Capacity[level];
                if (type == BuildingEnums.Granary) granary += Capacity[step.TargetLevel] - Capacity[level];
                projected[step.Slot] = new(step.Slot, type, step.TargetLevel);
                for (var i = 0; i < 4; i++) totals[i] += cost[i];
                rows.Add(new(rows.Count + 1, step.Slot, step.BuildingId, type.ToString(), level, step.TargetLevel,
                    $"{cost[0]:N0} / {cost[1]:N0} / {cost[2]:N0} / {cost[3]:N0}", step.Reason));
            }
            if (snapshot.UnderAttack) warnings.Add("Incoming attack detected. Review immediate defense and resource reserves before applying.");
            if (snapshot.Resources.FreeCrop <= 0) warnings.Add("Free crop is exhausted; crop production may need upgrading before other construction.");
            if (snapshot.ProductionPerHour is { Length: 4 } rates)
            {
                if (rates[3] < 0) warnings.Add("Net crop production is negative. The army may need additional crop supply.");
                var needed = Enumerable.Range(0, 4).Where(i => totals[i] > snapshot.Resources.Stock[i] + Math.Max(0, rates[i]) * options.HorizonHours).ToArray();
                if (needed.Length > 0) warnings.Add("Total upgrade costs exceed current stocks plus current production over the chosen horizon. Future production, construction time and training expenditure can change this estimate.");
            }
            else warnings.Add("Hourly production could not be read. Resource funding time is unknown.");
            if (options.ApplyMode == QueueApplyMode.Append && snapshot.PendingJobs.Length > 0)
                warnings.Add("This plan starts after the pending queue. Its existing costs and completion time are additional.");
            if (options.Goal is PlannerGoal.NewVillage or PlannerGoal.Conqueror)
                warnings.Add("Culture points, occupied expansion slots and settlers/chiefs are not verified automatically. Confirm them in-game; building levels alone do not prove expansion readiness.");
            warnings.Add("Research, unit counts, construction duration and bonuses are not fully observed. This is a validated building recommendation, not a guaranteed completion schedule.");
            return new([.. rows], [.. errors], [.. warnings], totals);
        }

        private static long StorageDelta(IEnumerable<PlannerBuilding> current, IEnumerable<PlannerBuilding> projected, BuildingEnums type)
        {
            static long Sum(IEnumerable<PlannerBuilding> buildings, BuildingEnums kind) => buildings.Where(x => x.Type == kind).Sum(x => Capacity[Math.Clamp(x.Level, 0, 20)]);
            return Math.Max(0, Sum(projected, type) - Sum(current, type));
        }

        public static Dictionary<int, PlannerBuilding> Project(VillageSnapshot snapshot, QueueApplyMode mode)
        {
            var result = snapshot.Buildings.ToDictionary(x => x.Slot);
            foreach (var queued in snapshot.Construction)
            {
                var slot = queued.Slot;
                if (slot < 0)
                {
                    var matches = result.Values.Where(x => x.Type == queued.Type && x.Level < queued.Level).ToArray();
                    if (matches.Length != 1) throw new InvalidOperationException("An active construction has an ambiguous slot. Refresh after it completes.");
                    slot = matches[0].Slot;
                }
                if (!result.TryGetValue(slot, out var old) || old.Type != queued.Type && old.Type != BuildingEnums.Site)
                    throw new InvalidOperationException("Active construction no longer matches the village layout.");
                result[slot] = new(slot, queued.Type, Math.Max(old.Level, queued.Level));
            }
            if (mode == QueueApplyMode.ReplacePending) return result;
            try
            {
                foreach (var job in snapshot.PendingJobs)
                {
                    if (job.Type == JobTypeEnums.NormalBuild)
                    {
                        var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content) ?? throw new JsonException();
                        if (!result.TryGetValue(plan.Location, out var old) || old.Type != plan.Type && old.Type != BuildingEnums.Site || plan.Level is < 1 or > 100)
                            throw new InvalidOperationException("An existing job does not match the village layout. Correct it or choose Replace pending.");
                        result[plan.Location] = new(plan.Location, plan.Type, Math.Max(old.Level, plan.Level));
                    }
                    else if (job.Type == JobTypeEnums.ResourceBuild)
                    {
                        var plan = JsonSerializer.Deserialize<ResourceBuildPlan>(job.Content) ?? throw new JsonException();
                        if (!Enum.IsDefined(plan.Plan) || plan.Level is < 1 or > 100) throw new JsonException();
                        foreach (var field in result.Values.Where(x => x.Type.IsResourceField()).ToArray())
                        {
                            if (plan.Plan == ResourcePlanEnums.OnlyCrop && field.Type != BuildingEnums.Cropland || plan.Plan == ResourcePlanEnums.ExcludeCrop && field.Type == BuildingEnums.Cropland) continue;
                            result[field.Slot] = field with { Level = Math.Max(field.Level, plan.Level) };
                        }
                    }
                    else throw new InvalidOperationException("An existing job cannot be projected. Choose Replace pending or remove that job first.");
                }
            }
            catch (JsonException) { throw new InvalidOperationException("The pending queue contains invalid job data. Correct it before planning."); }
            return result;
        }
    }
}
