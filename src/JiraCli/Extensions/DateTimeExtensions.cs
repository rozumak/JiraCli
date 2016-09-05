using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraCli.Extensions
{
    public static class DateTimeExtensions
    {
        public static bool IsWeekendDay(this DateTime value, int workingDaysInWeek, DayOfWeek firstDayOfWeek)
        {
            DayOfWeek currentDayOfWeek = value.Date.DayOfWeek;
            DayOfWeek startDay = firstDayOfWeek;
            DayOfWeek endDay = firstDayOfWeek + workingDaysInWeek - 1;

            if (endDay < startDay)
                endDay += 7;

            if (currentDayOfWeek < startDay)
                currentDayOfWeek += 7;

            if (currentDayOfWeek >= startDay && currentDayOfWeek <= endDay)
            {
                return false;
            }

            return true;
        }
    }
}
