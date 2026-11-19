using System.Text.Json;

namespace MainCore.Shared.Dtos
{
    public class JobDto
    {
        public JobId Id { get; set; }
        public int Position { get; set; }
        public JobTypeEnums Type { get; set; }
        public required string Content { get; set; }

        public override string ToString()
        {
            switch (Type)
            {
                case JobTypeEnums.NormalBuild:
                    {
                        var plan = JsonSerializer.Deserialize<NormalBuildPlan>(Content);
                        return $"Build {plan.Type.Humanize()} to level {plan.Level} at location {plan.Location}";
                    }
                case JobTypeEnums.ResourceBuild:
                    {
                        var plan = JsonSerializer.Deserialize<ResourceBuildPlan>(Content);
                        return $"Build {plan.Plan.Humanize()} to level {plan.Level}";
                    }
                default:
                    return Content;
            }
        }

        public static string GetJobContent(Job job)
        {
            switch (job.Type)
            {
                case JobTypeEnums.NormalBuild:
                    {
                        var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content);
                        return $"Build {plan.Type.Humanize()} to level {plan.Level} at location {plan.Location}";
                    }
                case JobTypeEnums.ResourceBuild:
                    {
                        var plan = JsonSerializer.Deserialize<ResourceBuildPlan>(job.Content);
                        return $"Build {plan.Plan.Humanize()} to level {plan.Level}";
                    }
                default:
                    return job.Content;
            }
        }
    }

    public record struct NormalBuildPlan(BuildingEnums Type, int Location, int Level)
    {
        public override readonly string ToString()
        {
            return $"{Type.Humanize()} at slot {Location} to level {Level}";
        }
    }
    public record struct ResourceBuildPlan(ResourcePlanEnums Plan, int Level)
    {
        public override readonly string ToString()
        {
            return $"{Plan.Humanize()} to level {Level}";
        }
    }
}