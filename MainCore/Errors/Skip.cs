﻿namespace MainCore.Errors
{
    public class Skip : Error
    {
        public Skip() : base()
        {
        }

        private Skip(string message) : base(message)
        {
        }

        public static Skip VillageNotFound => new("Village not found");
        public static Skip AccountLogout => new("Account is logout. Re-login now");

        public static Skip NoRallypoint => new("No rallypoint found. Recheck & load village has rallypoint in Village>Build tab");
        public static Skip NoActiveFarmlist => new("No farmlist is active");
        public static Skip NoAdventure => new("No adventure available");

        public static Skip OverflowNPC => new("Overflow NPC resources. Bot won't npc to save gold");
    }
}