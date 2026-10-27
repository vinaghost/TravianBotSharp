using System.ComponentModel.DataAnnotations;

namespace MainCore.Entities
{
    public sealed class AiPlanReceipt
    {
        [Key] public string PlanId { get; set; } = "";
        public int AccountId { get; set; }
        public int VillageId { get; set; }
        public string CreatedUtc { get; set; } = "";
        public string Goal { get; set; } = "";
        public string BeforeJobs { get; set; } = "";
        public string AfterJobs { get; set; } = "";
        public string Description { get; set; } = "";
        public bool Undone { get; set; }
    }
}
