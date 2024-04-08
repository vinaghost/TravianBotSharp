using FluentResults;

namespace MainCore.Common.Errors
{
    public class NoAccessAvailable : Error
    {
        private NoAccessAvailable(string message) : base(message)
        {
        }

        public static NoAccessAvailable AllAccessNotWorking => new("All accesses not working");
        public static NoAccessAvailable LackOfAccess => new("Last access is reused , it may get MH's attention");
    }
}