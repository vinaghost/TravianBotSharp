using FluentResults;

namespace MainCore.Common.Errors
{
    public class Cancel : Error
    {
        public Cancel() : base("Pause button is pressed")
        {
        }
    }
}