namespace MainCore.Errors
{
    public class Retry : Error
    {
        private Retry() : base("Bot must retry")
        {
        }

        public static Result Error => new Retry();
    }
}