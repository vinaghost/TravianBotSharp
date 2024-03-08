using Humanizer;
using MainCore.DTO;

namespace MainCore.UI.Models.Output
{
    public class AdventureItem
    {
        public AdventureItem(AdventureDto dto)
        {
            Coordinates = $"{dto.X} | {dto.Y}";
            Difficulty = dto.Difficulty.Humanize();
        }

        public string Coordinates { get; set; }
        public string Difficulty { get; set; }
    }
}