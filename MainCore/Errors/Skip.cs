namespace MainCore.Errors
{
    public class Skip : Error
    {
        private Skip() : base()
        {
        }

        public static Result Error => new Skip();
    }
}