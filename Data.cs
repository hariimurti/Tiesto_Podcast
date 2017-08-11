using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Tiesto.Podcast
{
    class Data
    {
        public static string Normalize(string text)
        {
            byte[] bytes = Encoding.GetEncoding(1252).GetBytes(text);
            return Encoding.UTF8.GetString(bytes);
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static string GetPerformer(string text)
        {
            var match = new Regex(@"(?:guest.+)?(?:mix.+)?[-–_].?(.+)", RegexOptions.IgnoreCase).Match(text);
            if (match.Success)
            {
                return Data.UpperCaseFirst(match.Groups[1].Value.Trim());
            }
            else
            {
                return string.IsNullOrWhiteSpace(text) ? "VA" : text ;
            }
        }

        public static string UpperCaseFirst(string text)
        {
            string result = null;
            string[] words = text.Split(' ');
            foreach (string word in words)
            {
                result += char.ToUpper(word[0]) + word.Substring(1) + " ";
            }
            return result.Trim();
        }
    }
}
