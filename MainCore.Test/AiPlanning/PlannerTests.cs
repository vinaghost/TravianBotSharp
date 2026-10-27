using System.Text.Json;
using MainCore.Entities;
using MainCore.Enums;
using MainCore.Models;
using MainCore.Services.AiPlanning;

namespace MainCore.Test.AiPlanning
{
    public static class PlannerFixture
    {
        public static VillageSnapshot Snapshot()
        {
            var buildings = Enumerable.Range(1, 40).Select(slot => new PlannerBuilding(slot,
                slot <= 18 ? (BuildingEnums)((slot - 1) % 4 + 1) : BuildingEnums.Site, slot <= 18 ? 1 : 0)).ToArray();
            buildings[18] = new(19, BuildingEnums.Warehouse, 20);
            buildings[19] = new(20, BuildingEnums.Granary, 20);
            buildings[20] = new(21, BuildingEnums.Residence, 9);
            buildings[25] = new(26, BuildingEnums.MainBuilding, 5);
            buildings[30] = new(31, BuildingEnums.Barracks, 3);
            buildings[38] = new(39, BuildingEnums.RallyPoint, 1);
            buildings[39] = new(40, BuildingEnums.EarthWall, 0);
            return new(1, 1, DateTimeOffset.UtcNow, TribeEnums.Teutons, false, false, false, buildings, [], [],
                new(12000, 12000, 12000, 12000, 80000, 80000, 100), [1000, 1000, 1000, 1000], "fixture");
        }

        public static AiProposal Proposal(params PlanStep[] steps) => new("Prepare the next settlement.", steps,
            ["Verify culture points and an unused expansion slot, then train three settlers."], ["No gold use."]);
        public static PlanStep Step(BuildingEnums type = BuildingEnums.Residence, int slot = 21, int level = 10) => new((int)type, slot, level, "Prepare expansion infrastructure.");
    }

    public class PlannerTests
    {
        [Fact]
        public void NoUpgradesCanBeAValidRecommendationButCannotBeApplied()
        {
            var result = PlannerValidator.Validate(PlannerFixture.Snapshot(), new(), PlannerFixture.Proposal());
            result.IsValid.ShouldBeTrue();
            result.CanApply.ShouldBeFalse();
            result.TotalCost.ShouldBe(new long[4]);
        }

        [Fact]
        public void AcceptsSettlementInfrastructureAndCalculatesCosts()
        {
            var result = PlannerValidator.Validate(PlannerFixture.Snapshot(), new(), PlannerFixture.Proposal(PlannerFixture.Step()));
            result.IsValid.ShouldBeTrue();
            result.TotalCost.ShouldBe(BuildingEnums.Residence.GetCost(10).Take(4));
            result.Warnings.ShouldContain(x => x.Contains("occupied expansion slots"));
        }

        [Theory]
        [InlineData(999, 21, 10)]
        [InlineData((int)BuildingEnums.Residence, 21, 11)]
        [InlineData((int)BuildingEnums.Residence, 21, 9)]
        [InlineData((int)BuildingEnums.MainBuilding, 1, 2)]
        [InlineData((int)BuildingEnums.Residence, 45, 1)]
        [InlineData((int)BuildingEnums.CityWall, 40, 1)]
        [InlineData((int)BuildingEnums.CommandCenter, 22, 1)]
        [InlineData((int)BuildingEnums.Palace, 22, 1)]
        [InlineData((int)BuildingEnums.Barracks, 22, 1)]
        [InlineData((int)BuildingEnums.Workshop, 22, 1)]
        public void RejectsIllegalProposals(int building, int slot, int level)
        {
            var result = PlannerValidator.Validate(PlannerFixture.Snapshot(), new(), PlannerFixture.Proposal(new PlanStep(building, slot, level, "Test")));
            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldNotBeEmpty();
        }

        [Fact]
        public void AcceptsPrerequisitesBuiltEarlierInTheSamePlan()
        {
            var result = PlannerValidator.Validate(PlannerFixture.Snapshot(), new(), PlannerFixture.Proposal(
                PlannerFixture.Step(BuildingEnums.Academy, 22, 1), PlannerFixture.Step(BuildingEnums.Smithy, 23, 1)));
            result.IsValid.ShouldBeTrue();
        }

        [Fact]
        public void RejectsWholePlanWhenOneStepFails()
        {
            var result = PlannerValidator.Validate(PlannerFixture.Snapshot(), new(), PlannerFixture.Proposal(
                PlannerFixture.Step(), PlannerFixture.Step(BuildingEnums.Stable, 22, 1)));
            result.Steps.Length.ShouldBe(1);
            result.IsValid.ShouldBeFalse();
        }

        [Fact]
        public void EnforcesStorageCapacityBeforeAnUpgrade()
        {
            var state = PlannerFixture.Snapshot() with { Resources = new(500, 500, 500, 500, 800, 800, 100) };
            PlannerValidator.Validate(state, new(), PlannerFixture.Proposal(PlannerFixture.Step())).IsValid.ShouldBeFalse();
        }

        [Fact]
        public void AllowsStorageUpgradeToUnlockTheNextStep()
        {
            var state = PlannerFixture.Snapshot();
            state.Buildings[18] = new(19, BuildingEnums.Warehouse, 2);
            state.Buildings[20] = new(21, BuildingEnums.Residence, 5);
            state = state with { Resources = new(1000, 1000, 1000, 1000, 1700, 80000, 100) };
            var result = PlannerValidator.Validate(state, new(), PlannerFixture.Proposal(
                PlannerFixture.Step(BuildingEnums.Warehouse, 19, 3), PlannerFixture.Step(BuildingEnums.Residence, 21, 6)));
            result.IsValid.ShouldBeTrue();
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        public void RequiresCapitalConfirmationForFieldsAboveTen(bool? capital, bool expected)
        {
            var state = PlannerFixture.Snapshot();
            state.Buildings[0] = new(1, BuildingEnums.Woodcutter, 10);
            var result = PlannerValidator.Validate(state, new() { IsCapital = capital }, PlannerFixture.Proposal(PlannerFixture.Step(BuildingEnums.Woodcutter, 1, 11)));
            result.IsValid.ShouldBe(expected);
        }

        [Fact]
        public void AppendProjectsExistingJobsButReplaceDoesNot()
        {
            var job = new NormalBuildPlan { Type = BuildingEnums.Residence, Location = 21, Level = 10 };
            var state = PlannerFixture.Snapshot() with { PendingJobs = [new(1, 0, JobTypeEnums.NormalBuild, JsonSerializer.Serialize(job))] };
            var proposal = PlannerFixture.Proposal(PlannerFixture.Step());
            PlannerValidator.Validate(state, new(), proposal).IsValid.ShouldBeFalse();
            PlannerValidator.Validate(state, new() { ApplyMode = QueueApplyMode.ReplacePending }, proposal).IsValid.ShouldBeTrue();
        }

        [Fact]
        public void ProjectsResourceJobsAndActiveConstruction()
        {
            var state = PlannerFixture.Snapshot() with
            {
                Construction = [new(21, BuildingEnums.Residence, 10, DateTime.Now.AddMinutes(2))],
                PendingJobs = [new(2, 0, JobTypeEnums.ResourceBuild, JsonSerializer.Serialize(new ResourceBuildPlan { Plan = ResourcePlanEnums.OnlyCrop, Level = 4 }))]
            };
            var projected = PlannerValidator.Project(state, QueueApplyMode.Append);
            projected[21].Level.ShouldBe(10);
            projected[4].Level.ShouldBe(4);
            projected[1].Level.ShouldBe(1);
        }

        [Fact]
        public void RejectsAmbiguousActiveConstruction()
        {
            var state = PlannerFixture.Snapshot() with { Construction = [new(-1, BuildingEnums.Woodcutter, 2, DateTime.Now.AddMinutes(1))] };
            PlannerValidator.Validate(state, new(), PlannerFixture.Proposal(PlannerFixture.Step())).IsValid.ShouldBeFalse();
        }

        [Fact]
        public void MissingAndExcessiveResponseFieldsCannotBeApplied()
        {
            var state = PlannerFixture.Snapshot();
            PlannerValidator.Validate(state, new(), new(null!, null!, null!, null!)).IsValid.ShouldBeFalse();
            PlannerValidator.Validate(state, new() { MaxSteps = 1 }, PlannerFixture.Proposal(PlannerFixture.Step(), PlannerFixture.Step())).IsValid.ShouldBeFalse();
        }

        [Fact]
        public void ContextContainsSettlementGoalGuideTextAndNoAccountIdentity()
        {
            var context = AiPlannerService.BuildContext(PlannerFixture.Snapshot(), new(), [new("Guide", "https://example.org", "Reference passage", "test")]);
            context.ShouldContain("settle another village");
            context.ShouldContain("Reference passage");
            context.ShouldNotContain("accountId");
            context.ShouldNotContain("villageId");
        }

        [Fact]
        public void RemovesScriptsAndMarkupFromGuideContext()
        {
            var result = GuideContextService.ExtractText("<nav>Menu</nav><article><script>evil()</script><p>Expand &amp; grow</p></article><footer>Footer</footer>");
            result.ShouldBe("Expand & grow");
        }
    }
}
