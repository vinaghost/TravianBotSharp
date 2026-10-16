using System.Globalization;
using System.Text;

namespace MainCore.Services
{
    public sealed record MapVillage(
        int X,
        int Y,
        int Tribe,
        int VillageId,
        string VillageName,
        int PlayerId,
        string PlayerName,
        int AllianceId,
        string AllianceTag,
        int Population);

    public sealed record InactiveVillage(
        MapVillage Village,
        double Distance,
        int PlayerPopulationBefore,
        int PlayerPopulationNow);

    /// <summary>
    /// <paramref name="OldSnapshot"/> is null when there was not enough history yet;
    /// the villages are then only filtered by distance and population.
    /// </summary>
    public sealed record InactiveSearchResult(
        DateOnly? OldSnapshot,
        DateOnly NewSnapshot,
        List<InactiveVillage> Villages);

    /// <summary>
    /// Every Travian server publishes all villages once a day in /map.sql.
    /// Snapshots are kept per server and day so a player whose population did not
    /// grow over several days can be found without loading any game page.
    /// </summary>
    [RegisterSingleton<MapDataService>]
    public sealed class MapDataService
    {
        private const int NatarsPlayerId = 1;
        public const int DefaultWorldSize = 401;
        private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromMinutes(2) };

        public static string GetFolder(string server)
        {
            var host = new Uri(server).Host;
            return Path.Combine(AppPaths.Root, "MapData", host);
        }

        private static string GetSnapshotPath(string server, DateOnly date)
        {
            return Path.Combine(GetFolder(server), $"{date:yyyy-MM-dd}.sql");
        }

        public async Task<Result> DownloadToday(string server, CancellationToken cancellationToken)
        {
            var path = GetSnapshotPath(server, DateOnly.FromDateTime(DateTime.Now));
            if (File.Exists(path)) return Result.Ok();

            var url = new Uri(new Uri(server), "/map.sql");
            try
            {
                var content = await _httpClient.GetStringAsync(url, cancellationToken);
                if (!content.Contains("INSERT INTO", StringComparison.Ordinal))
                {
                    return Result.Fail($"{url} did not return map data");
                }

                Directory.CreateDirectory(GetFolder(server));
                var tempPath = $"{path}.tmp";
                await File.WriteAllTextAsync(tempPath, content, cancellationToken);
                File.Move(tempPath, path, overwrite: true);
                return Result.Ok();
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or IOException)
            {
                return Result.Fail($"Cannot download {url}: {ex.Message}");
            }
        }

        public static List<DateOnly> GetSnapshotDates(string server)
        {
            var folder = GetFolder(server);
            if (!Directory.Exists(folder)) return [];

            return Directory.GetFiles(folder, "*.sql")
                .Select(x => DateOnly.TryParseExact(Path.GetFileNameWithoutExtension(x), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : (DateOnly?)null)
                .OfType<DateOnly>()
                .Order()
                .ToList();
        }

        /// <summary>
        /// Finds villages of players whose total population did not grow between a snapshot
        /// at least <paramref name="days"/> days old and the newest snapshot.
        /// </summary>
        public Result<InactiveSearchResult> FindInactives(
            string server,
            int centerX,
            int centerY,
            int radius,
            int minPopulation,
            int maxPopulation,
            int minPlayerPopulation,
            int maxPlayerPopulation,
            int days,
            IReadOnlyCollection<(int X, int Y)> ownVillages)
        {
            var dates = GetSnapshotDates(server);
            if (dates.Count == 0)
            {
                return Result.Fail<InactiveSearchResult>("No map data yet. Map data is downloaded once a day while the account is logged in.");
            }

            var newest = dates[^1];
            var oldDates = dates.Where(x => x.DayNumber <= newest.DayNumber - days).ToList();
            DateOnly? older = oldDates.Count == 0 ? null : oldDates[^1];

            var newVillages = Parse(GetSnapshotPath(server, newest));
            var newPopulation = GetPlayerPopulation(newVillages);
            // without enough history every player passes the inactive check
            var oldPopulation = older is null ? newPopulation : GetPlayerPopulation(Parse(GetSnapshotPath(server, older.Value)));

            var ownCoordinates = ownVillages.ToHashSet();
            var ownPlayerIds = newVillages
                .Where(x => ownCoordinates.Contains((x.X, x.Y)))
                .Select(x => x.PlayerId)
                .ToHashSet();

            var worldSize = GetWorldSize(newVillages);

            var villages = newVillages
                .Where(x => x.PlayerId != NatarsPlayerId && x.PlayerId != 0)
                .Where(x => !ownPlayerIds.Contains(x.PlayerId))
                .Where(x => x.Population >= minPopulation && x.Population <= maxPopulation)
                .Where(x => newPopulation[x.PlayerId] >= minPlayerPopulation && newPopulation[x.PlayerId] <= maxPlayerPopulation)
                .Where(x => oldPopulation.TryGetValue(x.PlayerId, out var before) && newPopulation[x.PlayerId] <= before)
                .Select(x => new InactiveVillage(
                    x,
                    GetDistance(centerX, centerY, x.X, x.Y, worldSize),
                    oldPopulation[x.PlayerId],
                    newPopulation[x.PlayerId]))
                .Where(x => x.Distance <= radius)
                .OrderBy(x => x.Distance)
                .ToList();

            return new InactiveSearchResult(older, newest, villages);
        }

        private static Dictionary<int, int> GetPlayerPopulation(List<MapVillage> villages)
        {
            return villages
                .GroupBy(x => x.PlayerId)
                .ToDictionary(x => x.Key, x => x.Sum(v => v.Population));
        }

        /// <summary>
        /// The map wraps around, so a 401x401 world (-200..200) has its edges next to each other.
        /// </summary>
        private static int GetWorldSize(List<MapVillage> villages)
        {
            var maxCoordinate = villages.Count == 0 ? 200 : villages.Max(x => Math.Max(Math.Abs(x.X), Math.Abs(x.Y)));
            return maxCoordinate * 2 + 1;
        }

        public static double GetDistance(int x1, int y1, int x2, int y2, int worldSize)
        {
            var dx = Math.Abs(x1 - x2);
            var dy = Math.Abs(y1 - y2);
            dx = Math.Min(dx, worldSize - dx);
            dy = Math.Min(dy, worldSize - dy);
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public static List<MapVillage> Parse(string path)
        {
            var villages = new List<MapVillage>();
            foreach (var line in File.ReadLines(path))
            {
                var start = line.IndexOf("VALUES (", StringComparison.Ordinal);
                if (start < 0) continue;

                var values = SplitValues(line, start + "VALUES (".Length);
                // (fieldId, x, y, tribe, villageId, villageName, playerId, playerName, allianceId, allianceTag, population, ...)
                if (values.Count < 11) continue;

                villages.Add(new MapVillage(
                    X: ParseInt(values[1]),
                    Y: ParseInt(values[2]),
                    Tribe: ParseInt(values[3]),
                    VillageId: ParseInt(values[4]),
                    VillageName: values[5],
                    PlayerId: ParseInt(values[6]),
                    PlayerName: values[7],
                    AllianceId: ParseInt(values[8]),
                    AllianceTag: values[9],
                    Population: ParseInt(values[10])));
            }
            return villages;
        }

        private static int ParseInt(string value)
        {
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : 0;
        }

        /// <summary>
        /// Splits a SQL value tuple. Strings are single quoted and may contain \' or '' escapes and commas.
        /// </summary>
        private static List<string> SplitValues(string line, int index)
        {
            var values = new List<string>();
            var current = new StringBuilder();
            var inString = false;

            for (var i = index; i < line.Length; i++)
            {
                var c = line[i];
                if (inString)
                {
                    if (c == '\\' && i + 1 < line.Length)
                    {
                        current.Append(line[++i]);
                    }
                    else if (c == '\'' && i + 1 < line.Length && line[i + 1] == '\'')
                    {
                        current.Append('\'');
                        i++;
                    }
                    else if (c == '\'')
                    {
                        inString = false;
                    }
                    else
                    {
                        current.Append(c);
                    }
                    continue;
                }

                switch (c)
                {
                    case '\'':
                        inString = true;
                        break;

                    case ',':
                        values.Add(current.ToString().Trim());
                        current.Clear();
                        break;

                    case ')':
                        values.Add(current.ToString().Trim());
                        return values;

                    default:
                        current.Append(c);
                        break;
                }
            }
            return values;
        }
    }
}
