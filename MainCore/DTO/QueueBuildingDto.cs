#nullable disable

namespace MainCore.DTO
{
    public class QueueBuildingDto
    {
        public int Position { get; set; }

        public int Location { get; set; } = -1;
        public string Type { get; set; }
        public int Level { get; set; }
        public DateTime CompleteTime { get; set; }
    }
}