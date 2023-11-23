using FluentResults;

namespace MainCore.Common.Errors.Storage
{
    public class GranaryLimit : Error
    {
        public GranaryLimit(long storage, long required) : base($"Don't have enough storage [{storage} < {required}]")
        {
        }
    }
}