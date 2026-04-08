using System;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Apieducation.Helpers
{
    public static class JsonHelpers
    {
        public static bool TryExtractJson(string input, out string json)
        {
            json = string.Empty;
            if (string.IsNullOrWhiteSpace(input))
                return false;

            int first = input.IndexOf('{');
            int last = input.LastIndexOf('}');
            if (first != -1 && last != -1 && last > first)
            {
                var candidate = input.Substring(first, last - first + 1);
                if (IsValidJson(candidate))
                {
                    json = candidate;
                    return true;
                }
            }

            var match = Regex.Match(input, @"\{[\s\S]*?\}");
            while (match.Success)
            {
                if (IsValidJson(match.Value))
                {
                    json = match.Value;
                    return true;
                }
                match = match.NextMatch();
            }

            return false;
        }

        private static bool IsValidJson(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;

            try
            {
                using var doc = JsonDocument.Parse(s);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
