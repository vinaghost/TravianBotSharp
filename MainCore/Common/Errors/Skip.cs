using FluentResults;

namespace MainCore.Common.Errors
{
    public class Skip : Error
    {
        public Skip(string message) : base(message)
        {
        }

        public static Skip VillageNotFound => new("Village not found");
        public static Result AutoBuilderJobQueueEmpty => Result.Fail(new Skip("Auto builder job queue is empty"));
        public static Skip BuildingQueueFull => new("Building queue is full");
        public static Skip AccountLogout => new("Account is logout.");
    }
}