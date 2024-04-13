using MainCore.Common.Enums;

namespace MainCore.UI.Models.Output
{
    public class BonusItem
    {
        public BonusItem(AllianceBonusEnums bonus)
        {
            Bonus = bonus;
        }

        public AllianceBonusEnums Bonus { get; }
    }
}