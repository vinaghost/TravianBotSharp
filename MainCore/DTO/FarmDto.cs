#nullable disable

using Riok.Mapperly.Abstractions;

namespace MainCore.DTO
{
    public class FarmDto
    {
        public FarmId Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
    }
}