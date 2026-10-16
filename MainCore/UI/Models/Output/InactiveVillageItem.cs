namespace MainCore.UI.Models.Output
{
    public sealed record InactiveVillageItem(
        string Distance,
        string Coordinates,
        string Village,
        string Player,
        string Alliance,
        int Population,
        string PlayerPopulation);

    public sealed record InactiveSearchOutput(string Status, List<InactiveVillageItem> Villages);
}
