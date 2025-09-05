namespace MainCore.Enums
{
    public enum TroopEnums
    {
        None,

        //Romans//,
        Legionnaire,

        Praetorian,
        Imperian,
        EquitesLegati,
        EquitesImperatoris,
        EquitesCaesaris,
        RomanRam,
        RomanCatapult,
        RomanChief,
        RomanSettler,

        //Teutons//,
        Clubswinger,

        Spearman,
        Axeman,
        Scout,
        Paladin,
        TeutonicKnight,
        TeutonRam,
        TeutonCatapult,
        TeutonChief,
        TeutonSettler,

        //Gauls//,
        Phalanx,

        Swordsman,
        Pathfinder,
        TheutatesThunder,
        Druidrider,
        Haeduan,
        GaulRam,
        GaulCatapult,
        GaulChief,
        GaulSettler,

        //Nature//,
        Rat,

        Spider,
        Snake,
        Bat,
        WildBoar,
        Wolf,
        Bear,
        Crocodile,
        Tiger,
        Elephant,

        //Natars//,
        Pikeman,

        ThornedWarrior,
        Guardsman,
        BirdsOfPrey,
        Axerider,
        NatarianKnight,
        Warelephant,
        Ballista,
        NatarianEmperor,
        Settler,

        //Egyptians//,
        SlaveMilitia,

        AshWarden,
        KhopeshWarrior,
        SopduExplorer,
        AnhurGuard,
        ReshephChariot,
        EgyptianRam,
        EgyptianCatapult,
        EgyptianChief,
        EgyptianSettler,

        //Huns//,
        Mercenary,

        Bowman,
        Spotter,
        SteppeRider,
        Marksman,
        Marauder,
        HunRam,
        HunCatapult,
        HunChief,
        HunSettler,

        //Hero
        Hero
    }

    public static class TroopExtension
    {
        public static TribeEnums GetTribe(this TroopEnums troop)
        {
            return (int)troop switch
            {
                >= (int)TroopEnums.Legionnaire and <= (int)TroopEnums.RomanSettler => TribeEnums.Romans,
                >= (int)TroopEnums.Clubswinger and <= (int)TroopEnums.TeutonSettler => TribeEnums.Teutons,
                >= (int)TroopEnums.Phalanx and <= (int)TroopEnums.GaulSettler => TribeEnums.Gauls,
                >= (int)TroopEnums.Rat and <= (int)TroopEnums.Elephant => TribeEnums.Nature,
                >= (int)TroopEnums.Pikeman and <= (int)TroopEnums.Settler => TribeEnums.Natars,
                >= (int)TroopEnums.SlaveMilitia and <= (int)TroopEnums.EgyptianSettler => TribeEnums.Egyptians,
                >= (int)TroopEnums.Mercenary and <= (int)TroopEnums.HunSettler => TribeEnums.Huns,
                _ => TribeEnums.Any,
            };
        }
    }
}
