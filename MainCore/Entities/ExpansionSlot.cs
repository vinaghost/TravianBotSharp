using MainCore.Common.Enums;

namespace MainCore.Entities
{
    public class ExpansionSlot
    {
        public int Id { get; set; }
        public int VillageId { get; set; }
        public ExpansionStatusEnum Status { get; set; }
        public string Content { get; set; }
    }
}