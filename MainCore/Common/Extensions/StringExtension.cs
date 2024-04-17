using System.Web;

namespace MainCore.Common.Extensions
{
    public static class StringExtension
    {
        public static TimeSpan ToDuration(this string value)
        {
            //00:00:02 (+332 ms), TTWars, milliseconds matter
            int ms = 0;
            if (value.Contains("(+"))
            {
                var parts = value.Split('(');
                ms = parts[1].ToInt();
                value = parts[0];
            }
            // h:m:s
            var arr = value.Split(':');
            var h = arr[0].ToInt();
            var m = arr[1].ToInt();
            var s = arr[2].ToInt();
            return new TimeSpan(0, h, m, s, ms);
        }

        public static int ToInt(this string value)
        {
            value = HttpUtility.HtmlDecode(value);
            var valueStr = new string(value.Where(c => char.IsDigit(c) || c == '-' || c == '−').ToArray());
            valueStr = valueStr.Replace('−', '-');
            if (string.IsNullOrEmpty(valueStr)) return 0;
            return int.Parse(valueStr);
        }

        public static long ToLong(this string value)
        {
            value = HttpUtility.HtmlDecode(value);
            var valueStr = new string(value.Where(c => char.IsDigit(c) || c == '-' || c == '−').ToArray());
            valueStr = valueStr.Replace('−', '-');
            if (string.IsNullOrEmpty(valueStr)) return 0;
            return long.Parse(valueStr);
        }
    }
}