public static class TimeHelper
{
    public static DateTime ToIST(this DateTime utcTime)
    {
        var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcTime, DateTimeKind.Utc), istZone);
    }
}
