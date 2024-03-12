using Humanizer;
using MainCore.DTO;

namespace MainCore.UI.Models.Output
{
    public class HeroItemItem
    {
        public HeroItemItem(HeroItemDto dto)
        {
            Type = dto.Type.Humanize();
            Amount = dto.Amount;
        }

        public string Type { get; set; }
        public int Amount { get; set; }
    }
}