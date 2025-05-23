﻿using System.Text.RegularExpressions;

namespace MainCore.Common.Extensions
{
    public static partial class StringExtension
    {
        [GeneratedRegex(@"[^a-zA-Z0-9\s]")]
        private static partial Regex NonAlphanumericRegex();

        public static string Sanitize(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            return NonAlphanumericRegex().Replace(input, "").Replace(' ', '_');
        }

        public static string GetServerUrl(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            if (!Uri.TryCreate(input, UriKind.Absolute, out var uri))
                return "";

            return $"{uri.Scheme}://{uri.Host}";
        }
    }
}