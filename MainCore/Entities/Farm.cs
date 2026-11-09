using StronglyTypedIds;

namespace MainCore.Entities
{
    public class Farm
    {
        public int Id { get; set; }

        public int AccountId { get; set; }
        public required string Name { get; set; }
        public bool IsActive { get; set; }
    }

    [StronglyTypedId]
    public partial struct FarmId
    { }

    public class FarmDto
    {
        public FarmId Id { get; set; }
        public required string Name { get; set; }
        public bool IsActive { get; set; }
    }
}