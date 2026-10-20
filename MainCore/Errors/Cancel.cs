namespace MainCore.Errors
{
    public class Cancel : Error
    {
        private Cancel() : base("Pause button is pressed")
        {
        }

        public static Result Error => new Cancel();
    }
}