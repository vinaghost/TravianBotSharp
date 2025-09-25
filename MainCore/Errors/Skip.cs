namespace MainCore.Errors
{
    public class Skip : Error
    {
        private Skip() : base("Bot skip this task")
        {
        }

        public static Result Error => new Skip();
    }
}