using System;
using System.Globalization;

namespace Mercury
{
    public static class MercuryClassExtensions
    {
        #region DATETIME
        // 11:30 AM
        public static string ToString_HHMMTT(this DateTime dateTime)
        {
            return dateTime.ToString("hh:mm tt", CultureInfo.InvariantCulture);
        }

        // 11:30:59
        public static string ToString_HHMMSS(this DateTime dateTime)
        {
            return $"{dateTime.Day:D2}:{dateTime.Month:D2}:{dateTime.Second:D2}";
        }
    
        // 31.12.21
        public static string ToString_DDMMYY(this DateTime dateTime)
        {
            return $"{dateTime.Day:D2}.{dateTime.Month:D2}.{dateTime.Year:D2}";
        }
    
        // 31.12.21 - 11:30:59
        public static string ToString_DDMMYYHHMMSS(this DateTime dateTime)
        {
            return $"{ToString_DDMMYY(dateTime)} - {ToString_HHMMSS(dateTime)}";
        }
    
        // 31.12.2021 11:30 AM
        public static string ToString_DDMMYYYYHHMMTT(this DateTime dateTime)
        {
            return dateTime.ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture);
        }
        #endregion

        #region TIMESPAN
        // 11:30:59
        public static string ToString_HHMMSS(this TimeSpan timeSpan)
        {
            return $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }
    
        //31.11:30:59
        public static string ToString_DDHHMMSS(this TimeSpan timeSpan)
        {
            return $"{timeSpan.Days}.{ToString_HHMMSS(timeSpan)}";
        }

        // 24h = 1
        public static string ToString_DaysF2(this TimeSpan timeSpan)
        {
            return $"{(timeSpan.Hours / 24f):F2}";
        }
        #endregion
    }
}
