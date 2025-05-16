namespace MainCore.Models
{
    public class BuildingItem
    {
        public BuildingId Id { get; set; }
        public int Location { get; set; }
        public BuildingEnums Type { get; set; }

        public int CurrentLevel { get; set; }
        public int QueueLevel { get; set; }
        public int JobLevel { get; set; }

        public int Level => Math.Max(Math.Max(CurrentLevel, QueueLevel), JobLevel);
    }
}