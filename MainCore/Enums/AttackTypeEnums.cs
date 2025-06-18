namespace MainCore.Enums
{
    public enum AttackTypeEnums
    {
        /// <summary>
        /// Send troops as reinforcement. This corresponds to value="5" on the
        /// rally point send troops page.
        /// </summary>
        Reinforcement = 5,

        /// <summary>
        /// Normal attack. This corresponds to value="3".
        /// </summary>
        Attack = 3,

        /// <summary>
        /// Raid attack. This corresponds to value="4".
        /// </summary>
        Raid = 4,
    }
}
