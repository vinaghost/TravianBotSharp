using FluentResults;

namespace MainCore.Common.Errors.Farming
{
    public class NoRallypoint : Error
    {
        public NoRallypoint() : base("No rallypoint found. Recheck & load village has rallypoint in Village>Build tab")
        {
        }
    }
}