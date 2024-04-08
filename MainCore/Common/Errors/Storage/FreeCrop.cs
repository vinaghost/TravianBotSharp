using FluentResults;

namespace MainCore.Common.Errors.Storage
{
    public class FreeCrop : Error
    {
        private FreeCrop(long storage, long required) : base($"Don't have enough freecrop [{storage} < {required}]")
        {
        }

        public static FreeCrop Error(long storage, long required) => new(storage, required);
    }
}