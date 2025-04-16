﻿using System.Net;
using System.Text.RegularExpressions;

namespace MainCore.Common.Extensions
{
    public static partial class StringExtension
    {
        public static TimeSpan ToDuration(this string value)
        {
            //00:00:02 (+332 ms), TTWars, milliseconds matter
            int ms = 0;
            if (value.Contains("(+"))
            {
                var parts = value.Split('(');
                ms = parts[1].ParseInt();
                value = parts[0];
            }
            // h:m:s
            var arr = value.Split(':');
            var h = arr[0].ParseInt();
            var m = arr[1].ParseInt();
            var s = arr[2].ParseInt();
            return new TimeSpan(0, h, m, s, ms);
        }

        private static string Normalized(this string value)
        {
            var valueStrDecoded = WebUtility.HtmlDecode(value);
            if (string.IsNullOrEmpty(valueStrDecoded)) return "";

            var valueStr = new string(valueStrDecoded.Where(c => char.IsDigit(c) || c == '-' || c == '−').ToArray());
            valueStr = valueStr.Replace('−', '-');

            if (string.IsNullOrEmpty(valueStr)) return "";
            return valueStr;
        }

        public static int ParseInt(this string value)
        {
            var normValue = value.Normalized();
            if (string.IsNullOrEmpty(normValue)) return -1;
            return int.Parse(normValue);
        }

        public static long ParseLong(this string value)
        {
            var normValue = value.Normalized();
            if (string.IsNullOrEmpty(normValue)) return -1;
            return long.Parse(normValue);
        }

        [GeneratedRegex(@"[^a-zA-Z0-9\s]")]
        private static partial Regex NonAlphanumericRegex();

        public static string Sanitize(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            return NonAlphanumericRegex().Replace(input, "").Replace(' ', '_');
        }
    }
}