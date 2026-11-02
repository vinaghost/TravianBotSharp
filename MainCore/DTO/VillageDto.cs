#nullable disable

using Riok.Mapperly.Abstractions;

namespace MainCore.DTO
{
    public class VillageDto
    {
        public VillageId Id { get; set; }
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsActive { get; set; }
        public bool IsUnderAttack { get; set; }
    }
}