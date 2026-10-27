using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MainCore.Services.AiPlanning
{
    public sealed class GuideContextService
    {
        private static readonly HttpClient Client = new(new HttpClientHandler { AllowAutoRedirect = false }) { Timeout = TimeSpan.FromSeconds(15) };
        private sealed record CachedGuide(DateTimeOffset FetchedAt, string Text);
        private static readonly GuidePassage[] References =
        [
            new("Fast second village — guide index", "https://unofficialtravian.com/2025/01/guides-fast-second-village/",
                "This is a directory of player guides for different server speeds and raiding/non-raiding starts. Select advice matching speed and playstyle. Settlement needs expansion capacity, culture points, three settlers and resources. Do not infer unused expansion slots from building level alone.", "Bundled summary"),
            new("Non-raiding start for x3/x5/x10", "https://unofficialtravian.com/2025/01/guide-start-x3-x5-non-raiding-version/",
                "A non-raiding startup guide linked by the second-village directory for x3, x5 and x10 servers. Treat milestones as strategy references; validate requirements using the current game and village state.", "Bundled summary"),
            new("Travian Manual — historical strategy", "https://travianmanual.wordpress.com/",
                "Historical guides include offensive army development. Sustainable army production requires balancing training capacity, resources and crop upkeep. Much of this material dates to 2009; numerical values, tribe coverage and mechanics may be outdated. Prefer current rules over old guide numbers.", "Bundled summary")
        ];

        public async Task<GuidePassage[]> GetAsync(PlannerOptions options, CancellationToken token)
        {
            var chosen = References.Where((_, index) => index != 1 || options.Goal == PlannerGoal.NewVillage && options.ServerSpeed is 3 or 5 or 10);
            return await Task.WhenAll(chosen.Select((guide) => ReadAsync(guide, token)));
        }

        private static async Task<GuidePassage> ReadAsync(GuidePassage guide, CancellationToken token)
        {
            var file = Path.Combine(PlannerSettingsStore.DataDirectory, "guides", Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(guide.Url))) + ".json");
            CachedGuide? cached = null;
            try
            {
                if (File.Exists(file)) cached = JsonSerializer.Deserialize<CachedGuide>(await File.ReadAllTextAsync(file, token), PlannerJson.Options);
                if (cached is not null && DateTimeOffset.UtcNow - cached.FetchedAt < TimeSpan.FromDays(7))
                    return guide with { Text = guide.Text + "\n\n" + cached.Text, Status = $"Cached {cached.FetchedAt:yyyy-MM-dd}" };
                using var response = await Client.GetAsync(guide.Url, HttpCompletionOption.ResponseHeadersRead, token);
                response.EnsureSuccessStatusCode();
                await using var stream = await response.Content.ReadAsStreamAsync(token);
                using var reader = new StreamReader(stream);
                var chars = new char[500_001];
                var read = await reader.ReadBlockAsync(chars, token);
                if (read > 500_000) throw new IOException("Guide exceeds size limit.");
                var text = ExtractText(new string(chars, 0, read));
                if (text.Length < 150) throw new IOException("Guide has too little readable text.");
                cached = new(DateTimeOffset.UtcNow, text);
                Directory.CreateDirectory(Path.GetDirectoryName(file)!);
                await File.WriteAllTextAsync(file, PlannerJson.Write(cached), token);
                return guide with { Text = guide.Text + "\n\n" + text, Status = $"Retrieved {cached.FetchedAt:yyyy-MM-dd}" };
            }
            catch (OperationCanceledException) when (!token.IsCancellationRequested) { }
            catch (Exception ex) when (ex is HttpRequestException or IOException or UnauthorizedAccessException or JsonException or RegexMatchTimeoutException) { }
            return cached is null ? guide with { Status = "Bundled summary; website unavailable" }
                : guide with { Text = guide.Text + "\n\n" + cached.Text, Status = $"Older cache {cached.FetchedAt:yyyy-MM-dd}; website unavailable" };
        }

        public static string ExtractText(string html)
        {
            var article = Regex.Match(html, @"<article\b[^>]*>([\s\S]*?)</article>", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
            if (article.Success) html = article.Groups[1].Value;
            html = Regex.Replace(html, @"<(script|style|nav|header|footer)\b[^>]*>[\s\S]*?</\1>", " ", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
            html = Regex.Replace(html, @"<[^>]+>", " ", RegexOptions.None, TimeSpan.FromSeconds(1));
            var text = Regex.Replace(WebUtility.HtmlDecode(html), @"\s+", " ", RegexOptions.None, TimeSpan.FromSeconds(1)).Trim();
            return text[..Math.Min(text.Length, 6500)];
        }
    }
}
