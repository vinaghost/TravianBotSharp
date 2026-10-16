namespace MainCore.UI.Models.Output
{
    public sealed record InactiveVillageItem(
        string Distance,
        string Coordinates,
        string Type,
        string Village,
        string Player,
        string Alliance,
        string Population,
        string PlayerPopulation,
        string Animals,
        string LastAttack);

    public sealed record InactiveSearchOutput(string Status, List<InactiveVillageItem> Villages);
}
