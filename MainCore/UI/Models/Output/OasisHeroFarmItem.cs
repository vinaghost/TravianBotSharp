namespace MainCore.UI.Models.Output
{
    public class OasisHeroFarmItem
    {
        public int Id { get; set; }
        public string Type { get; set; } = "";
        public int X { get; set; }
        public int Y { get; set; }
        public string Animal { get; set; } = "";
        public string Resource { get; set; } = "";
        public DateTime DateTime { get; set; } = DateTime.MinValue;
    }
}