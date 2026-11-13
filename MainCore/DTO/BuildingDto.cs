namespace MainCore.DTO
{
    public class BuildingDto
    {
        public BuildingEnums Type { get; set; }
        public int Level { get; set; }
        public bool IsUnderConstruction { get; set; }
        public int Location { get; set; }
    }
}