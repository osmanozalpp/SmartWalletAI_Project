using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Common.Helpers
{
    public static class DateTimeExtensions
    {
        public static DateTime ToTurkeyTime(this DateTime utcDate)
        {
          
            var turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");

            return TimeZoneInfo.ConvertTimeFromUtc(utcDate, turkeyTimeZone);
        }
    }
}
