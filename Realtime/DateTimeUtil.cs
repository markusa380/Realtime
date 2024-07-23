using System;

namespace Realtime
{
    internal class DateTimeUtil
    {
        public static string ToISO8601(DateTime dt)
        {
            return dt.ToUniversalTime().ToString("o");
        }

        public static DateTime FromISO8601(string str)
        {
            return DateTime.ParseExact(str, "o", null).ToUniversalTime();
        }
    }
}
