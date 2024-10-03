﻿namespace MainCore.Common.Errors
{
    public class Skip : Error
    {
        private Skip(string message) : base(message)
        {
        }

        public static Skip Cancel => new("Cancel current task");
        public static Skip VillageNotFound => new("Village not found");
        public static Skip AutoBuilderJobQueueEmpty => new("Job queue is empty");
        public static Skip AutoBuilderNotEnoughResource => new("Reschedule becasue doesn't have enough resource");
        public static Skip AutoBuilderBuildingQueueFull => new("Building queue is full");
        public static Skip AccountLogout => new("Account is logout. Re-login now");

        public static Skip NoRallypoint => new("No rallypoint found. Recheck & load village has rallypoint in Village>Build tab");
        public static Skip NoActiveFarmlist => new("No farmlist is active");
        public static Skip NoAdventure => new("No adventure available");
    }
}