using TaskMonitoringApp.Models.Entities;

namespace TaskMonitoringApp.Utility
{
    public static class DateTimeUtility
    {
        public static Weekly GetTodayDayName()
        {
            // Get the current day of the week (Sunday = 0, Monday = 1, ..., Saturday = 6)
            var today = DateTime.Now.DayOfWeek;

            // Map the .NET DayOfWeek enum to our Weekly enum
            return (Weekly)((int)today == 0 ? 6 : (int)today - 1); // Adjust for Sunday being 0 in DayOfWeek, but 6 in Weekly enum
        }
    }
}
