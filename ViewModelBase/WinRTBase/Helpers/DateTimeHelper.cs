using System;

namespace WinRTBase
{
    public static class DateTimeHelper
    {
        //private const DateTime _START = new DateTime(1970, 1, 1);
        private static DateTime __start = new DateTime(2013, 1, 1);

        public static DateTime Int2DateTime(int seconds)
        {
            DateTime objTime = __start.AddSeconds(seconds);
            return objTime;
        }

        public static int DateTime2Int(DateTime obj)
        {
            int seconds = (int)obj.Subtract(__start).TotalSeconds;
            return seconds;
        }
    }
}
