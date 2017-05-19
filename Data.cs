using System;
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

        public static string GetGuestMix(string text)
        {
            Regex regex = new Regex(@"([\s\S]+)-([\s\S]+)");
            Match match = regex.Match(text);
            if (match.Success)
            {
                string grp1 = match.Groups[1].Value.Trim();
                string grp2 = match.Groups[2].Value.Trim();
                if (!grp1.ToLower().Contains("guest"))
                {
                    return $"{grp1} - {grp2}";
                }
                else
                {
                    return $"{grp2} - {grp1}";
                }
            }
            else
            {
                if (!text.ToLower().Contains("guest"))
                {
                    text = text.Trim() + " - Guest Mix";
                }
                return text.Trim();
            }
        }
    }
}
