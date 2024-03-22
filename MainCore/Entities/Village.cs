using StronglyTypedIds;

namespace MainCore.Entities
{
    public class Village
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public bool IsActive { get; set; }
        public bool IsUnderAttack { get; set; }

        public int AccountId { get; set; }
        public ICollection<Building> Buildings { get; set; }
        public ICollection<QueueBuilding> QueueBuildings { get; set; }
        public ICollection<Job> Jobs { get; set; }
        public Storage Storage { get; set; }
        public ICollection<VillageSetting> VillageSetting { get; set; }
        public ICollection<ExpansionSlot> ExpansionSlots { get; set; }
    }

    [StronglyTypedId]
    public partial struct VillageId
    { }
}