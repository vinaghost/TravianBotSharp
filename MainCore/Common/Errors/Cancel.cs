namespace MainCore.Common.Errors
{
    public class Cancel : Error
    {
        private Cancel() : base("Pause button is pressed")
        {
        }

        public static Cancel Error => new();
    }
}