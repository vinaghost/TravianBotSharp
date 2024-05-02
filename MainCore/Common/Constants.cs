namespace MainCore.Common
{
    public static class Constants
    {
        public static ServerEnums Server
        {
            get
            {
#if TRAVIAN_OFFICIAL
                return ServerEnums.TravianOfficial;
#endif
#if TTWARS
                return ServerEnums.TTWars;
#endif
            }
        }
    }
}