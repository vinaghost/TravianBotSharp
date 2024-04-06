using FluentResults;

namespace MainCore.Common.Errors
{
    public class NoAccessAvailable : Error
    {
        public NoAccessAvailable(string message) : base(message)
        {
        }

        public static Result AllAccessNotWorking => new NoAccessAvailable("All accesses not working");
        public static Result LackOfAccess => new NoAccessAvailable("Last access is reused , it may get MH's attention");
    }
}