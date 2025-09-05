namespace MainCore.Errors
{
    public class LackOfFreeCrop : Error
    {
        protected LackOfFreeCrop(long storage, long required) : base($"Don't have enough freecrop need {required} but have {storage}")
        {
        }

        public static LackOfFreeCrop Error(long storage, long required) => new(storage, required);
    }
}
