using FluentResults;

namespace MainCore.Common.Errors
{
    public class Skip : Error
    {
        private Skip(string message) : base(message)
        {
        }

        public static Skip VillageNotFound => new("Village not found");
        public static Skip AutoBuilderJobQueueEmpty => new("Auto builder job queue is empty");
        public static Skip BuildingQueueFull => new("Building queue is full");
        public static Skip AccountLogout => new("Account is logout.");

        public static Skip NoRallypoint => new("No rallypoint found. Recheck & load village has rallypoint in Village>Build tab");
        public static Skip NoActiveFarmlist => new("No farmlist is active");
    }
}