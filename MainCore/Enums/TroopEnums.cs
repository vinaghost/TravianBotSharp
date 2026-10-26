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

        //Spartan//,
        Hoplite,

        Sentinel,
        Shieldsman,
        TwinsteelTherion,
        ElpidaRider,
        CorinthianCrusher,
        SpartanRam,
        SpartanCatapult,
        SpartanChief,
        SpartanSettler,
    }

    public static class TroopExtension
    {
        extension(TroopEnums troop)
        {
            public TribeEnums GetTribe()
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
                    >= (int)TroopEnums.Hoplite and <= (int)TroopEnums.SpartanSettler => TribeEnums.Spartan,
                    _ => TribeEnums.Any,
                };
            }
        }

        extension(TribeEnums tribe)
        {
            public List<TroopEnums> GetInfantryTroops()
            {
                return tribe switch
                {
                    TribeEnums.Romans =>
                    [
                        TroopEnums.Legionnaire,
                    TroopEnums.Praetorian,
                    TroopEnums.Imperian,
                ],
                    TribeEnums.Teutons =>
                    [
                        TroopEnums.Clubswinger,
                    TroopEnums.Spearman,
                    TroopEnums.Axeman,
                    TroopEnums.Scout,
                ],
                    TribeEnums.Gauls =>
                    [
                       TroopEnums.Phalanx,
                    TroopEnums.Swordsman,
                ],
                    TribeEnums.Nature => [],
                    TribeEnums.Natars => [],
                    TribeEnums.Egyptians =>
                    [
                        TroopEnums.SlaveMilitia,
                    TroopEnums.AshWarden,
                    TroopEnums.KhopeshWarrior,
                ],
                    TribeEnums.Huns =>
                    [
                        TroopEnums.Mercenary,
                    TroopEnums.Bowman,
                ],
                    TribeEnums.Spartan =>
                    [
                        TroopEnums.Hoplite,
                    TroopEnums.Sentinel,
                    TroopEnums.Shieldsman,
                    TroopEnums.TwinsteelTherion,
                ],
                    _ => [],
                };
            }

            public List<TroopEnums> GetCavalryTroops()
            {
                return tribe switch
                {
                    TribeEnums.Romans =>
                    [
                        TroopEnums.EquitesLegati,
                    TroopEnums.EquitesImperatoris,
                    TroopEnums.EquitesCaesaris,
                ],
                    TribeEnums.Teutons =>
                    [
                        TroopEnums.Paladin,
                    TroopEnums.TeutonicKnight,
                ],
                    TribeEnums.Gauls =>
                    [
                        TroopEnums.Pathfinder,
                    TroopEnums.TheutatesThunder,
                    TroopEnums.Druidrider,
                    TroopEnums.Haeduan,
                ],
                    TribeEnums.Nature => [],
                    TribeEnums.Natars => [],
                    TribeEnums.Egyptians =>
                    [
                        TroopEnums.SopduExplorer,
                    TroopEnums.AnhurGuard,
                    TroopEnums.ReshephChariot,
                ],
                    TribeEnums.Huns =>
                    [
                        TroopEnums.Spotter,
                    TroopEnums.SteppeRider,
                    TroopEnums.Marksman,
                    TroopEnums.Marauder,
                ],

                    TribeEnums.Spartan =>
                    [
                        TroopEnums.ElpidaRider,
                    TroopEnums.CorinthianCrusher,
                ],
                    _ => [],
                };
            }

            public List<TroopEnums> GetSiegeTroops()
            {
                return tribe switch
                {
                    TribeEnums.Romans =>
                    [
                        TroopEnums.RomanRam,
                    TroopEnums.RomanCatapult,
                ],
                    TribeEnums.Teutons =>
                    [
                        TroopEnums.TeutonRam,
                    TroopEnums.TeutonCatapult,
                ],
                    TribeEnums.Gauls =>
                    [
                        TroopEnums.GaulRam,
                    TroopEnums.GaulCatapult,
                ],
                    TribeEnums.Nature => [],
                    TribeEnums.Natars => [],
                    TribeEnums.Egyptians =>
                    [
                        TroopEnums.EgyptianRam,
                    TroopEnums.EgyptianCatapult
                    ],
                    TribeEnums.Huns =>
                    [
                        TroopEnums.HunRam,
                    TroopEnums.HunCatapult,
                ],
                    TribeEnums.Spartan =>
                    [
                        TroopEnums.SpartanRam,
                    TroopEnums.SpartanCatapult,
                ],
                    _ => [],
                };
            }
        }
    }
}