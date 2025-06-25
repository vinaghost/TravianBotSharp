using System.Net;
using System.Text;

namespace MainCore.Parsers
{
    public static class OasisParser
    {
        public static bool IsOasis(HtmlDocument doc)
        {
            var div = doc.GetElementbyId("tileDetails");

            if (div is null) return false;

            return div.HasClass("oasis");
        }

        public static string GetOasisType(HtmlDocument doc)
        {
            var table = doc.GetElementbyId("distribution");
            if (table is null) return "Unknown";

            var sb = new StringBuilder();
            foreach (var row in table.Descendants("tr"))
            {
                var cells = row.Descendants("td").ToList();
                if (cells.Count < 3) continue;
                var valueCell = WebUtility.HtmlDecode(cells[1].InnerText);
                var typeCell = cells[2].InnerText;
                sb.Append($"{valueCell} {typeCell}, ");
            }

            return sb.ToString().TrimEnd(',', ' ');
        }

        public static string GetOasisAnimal(HtmlDocument doc)
        {
            // there is two troop_info, 1 for troops in oasis, 1 for reports, no idea why
            var table = doc.DocumentNode
                .Descendants("table")
                .Where(x => x.Id == "troop_info")
                .FirstOrDefault();
            if (table is null) return "Unknown";

            var sb = new StringBuilder();

            foreach (var row in table.Descendants("tr").SkipLast(1))
            {
                var cells = row.Descendants("td").ToList();
                if (cells.Count < 3) continue;
                var countCell = cells[1].InnerText;
                var animalCell = cells[2].InnerText;
                sb.Append($"{countCell} {animalCell}, ");
            }

            return sb.ToString().TrimEnd(',', ' ');
        }

        public static HtmlNode? GetSimulateButton(HtmlDocument doc)
        {
            // there is two troop_info, 1 for troops in oasis, 1 for reports, no idea why
            var table = doc.DocumentNode
               .Descendants("table")
               .Where(x => x.Id == "troop_info")
               .FirstOrDefault();
            if (table is null) return null;

            var button = table.Descendants("a")
                .FirstOrDefault(a => a.HasClass("arrow"));

            return button;
        }
    }
}