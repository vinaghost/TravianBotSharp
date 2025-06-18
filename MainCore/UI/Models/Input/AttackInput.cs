using MainCore.Enums;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;

namespace MainCore.UI.Models.Input
{
    public partial class AttackInput : ViewModelBase
    {
        public AmountInputViewModel[] Troops { get; } = Enumerable.Range(0, 11)
            .Select(_ => new AmountInputViewModel()).ToArray();

        [Reactive]
        private int _x;

        [Reactive]
        private int _y;

        [Reactive]
        private AttackTypeEnums _attackType = AttackTypeEnums.Raid;

        [Reactive]
        private DateTime _executeDate = DateTime.Today;

        [Reactive]
        private DateTime _executeTime = DateTime.Now;

        public (int x, int y, AttackTypeEnums type, int[] troops, DateTime executeAt) Get()
        {
            var troopValues = Troops.Select(t => t.Get()).ToArray();
            var executeAt = ExecuteDate.Date.Add(ExecuteTime.TimeOfDay);
            return (X, Y, AttackType, troopValues, executeAt);
        }
    }
}
