using FluentResults;

namespace MainCore.Common.Errors.AutoBuilder
{
    public class JobCompleted : Error
    {
        public static Result Removed => Result.Fail(new JobCompleted());
    }
}