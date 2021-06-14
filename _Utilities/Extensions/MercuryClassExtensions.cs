using System;
using System.Globalization;
using UnityEngine;

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
            return dateTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
        }
    
        // 31.12.21
        public static string ToString_DDMMYYYY(this DateTime dateTime)
        {
            return dateTime.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
        }
    
        // 31.12.21 - 11:30:59
        public static string ToString_DDMMYYYYHHMMSS(this DateTime dateTime)
        {
            return dateTime.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
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
            return timeSpan.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
        }
    
        //31.11:30:59
        public static string ToString_DDHHMMSS(this TimeSpan timeSpan)
        {
            return timeSpan.ToString("dd.HH:mm:ss", CultureInfo.InvariantCulture);
        }

        // 24h = 1
        public static string ToString_DaysF2(this TimeSpan timeSpan)
        {
            return $"{(timeSpan.Hours / 24f):F2}";
        }
        #endregion
    }
}
