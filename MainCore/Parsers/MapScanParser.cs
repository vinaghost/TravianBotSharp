using System.Text.Json;
using System.Text.RegularExpressions;

namespace MainCore.Parsers
{
    public enum MapTileKind
    {
        Other,
        Village,
        FreeOasis,
        OccupiedOasis,
    }

    public enum AttackResult
    {
        WonWithoutLosses = 1,
        WonWithLosses = 2,
        Lost = 3,
    }

    public sealed record MapTile(
        int X,
        int Y,
        MapTileKind Kind,
        Dictionary<int, int> Animals,
        AttackResult? LastAttack,
        string LastAttackTime,
        string LastAttackLoot);

    /// <summary>
    /// Reads the tiles the game map itself loads from /api/v1/map/position.
    /// Zoom level 2 returns 21x17 tiles per request including free oases, their animals and
    /// the last report of your own attacks on each tile. The text uses language independent
    /// placeholders such as {k.fo} (free oasis) and {b:ri2} (last raid won with losses).
    /// </summary>
    public static partial class MapScanParser
    {
        private const int ZoomLevel = 2;
        private const int HalfWidth = 10;
        private const int HalfHeight = 8;

        public static readonly IReadOnlyDictionary<int, string> AnimalNames = new Dictionary<int, string>
        {
            { 31, "Rat" },
            { 32, "Spider" },
            { 33, "Snake" },
            { 34, "Bat" },
            { 35, "Wild Boar" },
            { 36, "Wolf" },
            { 37, "Bear" },
            { 38, "Crocodile" },
            { 39, "Tiger" },
            { 40, "Elephant" },
        };

        public record struct RawTile(int X, int Y, string? Title, string? Text);

        public static async Task<List<MapTile>> Scan(IPage page, int centerX, int centerY, int radius)
        {
            var tiles = new Dictionary<(int, int), MapTile>();
            foreach (var (x, y) in GetRequestCenters(centerX, centerY, radius))
            {
                var json = await page.EvaluateAsync<JsonElement>(@"async ([x, y, zoomLevel]) => {
                    const response = await fetch('/api/v1/map/position', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json; charset=UTF-8', 'X-Requested-With': 'XMLHttpRequest' },
                        body: JSON.stringify({ data: { x, y, zoomLevel, ignorePositions: [] } })
                    });
                    if (!response.ok) throw new Error('map request failed: ' + response.status);
                    const data = await response.json();
                    return (data.tiles || []).map(t => ({ X: t.position.x, Y: t.position.y, Title: t.title || '', Text: t.text || '' }));
                }", new[] { x, y, ZoomLevel });

                var rawTiles = JsonSerializer.Deserialize<List<RawTile>>(json.GetRawText()) ?? [];
                foreach (var raw in rawTiles)
                {
                    tiles[(raw.X, raw.Y)] = Parse(raw);
                }
            }
            return tiles.Values.ToList();
        }

        private static IEnumerable<(int X, int Y)> GetRequestCenters(int centerX, int centerY, int radius)
        {
            var width = HalfWidth * 2 + 1;
            var height = HalfHeight * 2 + 1;
            for (var x = centerX - radius + HalfWidth; x - HalfWidth <= centerX + radius; x += width)
            {
                for (var y = centerY - radius + HalfHeight; y - HalfHeight <= centerY + radius; y += height)
                {
                    yield return (x, y);
                }
            }
        }

        public static MapTile Parse(RawTile raw)
        {
            var title = raw.Title ?? "";
            var text = (raw.Text ?? "").Replace("&#x202d;", "").Replace("&#x202c;", "");

            var kind = title.Contains("{k.fo}") ? MapTileKind.FreeOasis
                : title.Contains("{k.bt}") ? MapTileKind.OccupiedOasis
                : title.Contains("{k.dt}") ? MapTileKind.Village
                : MapTileKind.Other;

            var animals = AnimalRegex().Matches(text)
                .ToDictionary(x => int.Parse(x.Groups[1].Value), x => int.Parse(x.Groups[2].Value));

            AttackResult? lastAttack = null;
            var lastAttackTime = "";
            var report = ReportRegex().Match(text);
            if (report.Success && int.TryParse(report.Groups[1].Value, out var result) && Enum.IsDefined(typeof(AttackResult), result))
            {
                lastAttack = (AttackResult)result;
                lastAttackTime = report.Groups[2].Value.Trim();
            }

            var loot = LootRegex().Match(text);
            var lastAttackLoot = loot.Success ? loot.Groups[1].Value.Trim() : "";

            return new MapTile(raw.X, raw.Y, kind, animals, lastAttack, lastAttackTime, lastAttackLoot);
        }

        [GeneratedRegex(@"unit u(\d+)""></i><span class=""value "">(\d+)")]
        private static partial Regex AnimalRegex();

        [GeneratedRegex(@"\{b:ri(\d)\}(.*?)\{b\.ri\d\}")]
        private static partial Regex ReportRegex();

        [GeneratedRegex(@"\{b:bi\d\}([^<{]*)")]
        private static partial Regex LootRegex();
    }
}
