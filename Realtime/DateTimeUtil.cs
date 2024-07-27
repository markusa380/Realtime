using System;

namespace Realtime
{
    internal class DateTimeUtil
    {
        public static string ToISO8601(DateTimeOffset dt)
        {
            return dt.ToUniversalTime().ToString("o");
        }

        private static readonly string HUMAN_READABLE_FORMAT = "yyyy-MM-dd HH:mm:sszzz";

        public static DateTimeOffset FromISO8601(string str)
        {
            return DateTimeOffset.ParseExact(str, "o", null).ToUniversalTime();
        }

        public static string ToHumanReadable(DateTimeOffset dt)
        {
            return dt.ToString(HUMAN_READABLE_FORMAT);
        }

        public static DateTimeOffset FromHumanReadable(string str)
        {
            return DateTimeOffset.ParseExact(str, HUMAN_READABLE_FORMAT, null);
        }

        public static DateTimeOffset Localize(DateTimeOffset dt, bool local)
        {
            return local ? dt.ToUniversalTime() : dt.ToLocalTime();
        }
    }
}
