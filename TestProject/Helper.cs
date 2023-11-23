using System.Text;

namespace TestProject
{
    public static class Helper
    {
        public static string[] GetParts<T>()
        {
            var ns = typeof(T).Namespace;
            var parts = ns
                 .Replace("TestProject.", "")
                 .Split('.');
            return parts;
        }

        public static string GetPath(string[] parts, string filename)
        {
            parts = parts
                .Append(filename)
                .ToArray();
            var path = Path.Combine(parts);
            return path;
        }
    }
}