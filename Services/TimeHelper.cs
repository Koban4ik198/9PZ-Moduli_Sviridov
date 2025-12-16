using System;
using System.Data.Entity;
namespace pz6.Helpers
{
    public static class TimeHelper
    {
        // Метод для определения времени суток
        public static string GetTimeOfDayGreeting()
        {
            var hour = DateTime.Now.Hour;

            if (hour >= 10 && hour < 12) return "Доброе утро";
            else if (hour >= 12 && hour <= 17) return "Добрый день";
            else if (hour > 17 && hour <= 19) return "Добрый вечер";
            else return "Здравйствйте"; // по умолчанию
        }

        // Метод для проверки рабочего времени (10:00-19:00)
        public static bool IsWorkingHours()
        {
            var now = DateTime.Now;
            var startTime = new DateTime(now.Year, now.Month, now.Day, 10, 0, 0);
            var endTime = new DateTime(now.Year, now.Month, now.Day, 23, 0, 0);

            return now >= startTime && now <= endTime;
        }
    }
}