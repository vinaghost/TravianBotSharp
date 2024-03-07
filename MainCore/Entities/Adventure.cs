using MainCore.Common.Enums;

namespace MainCore.Entities
{
    public class Adventure
    {
        public int AccountId { get; set; }
        public int Id { get; set; }

        public int X { get; set; }
        public int Y { get; set; }
        public DifficultyEnums Difficulty { get; set; }
    }
}