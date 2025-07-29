using System;

namespace LoanManagementSystem.Helpers
{
    public static class DateTimeExtensions
    {
        public static DateTime ToIST(this DateTime utcTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(
                utcTime, 
                TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")
            );
        }
    }
}