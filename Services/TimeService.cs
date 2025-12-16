// TimeService.cs
using System;

namespace pz6.Services
{
    public static class TimeService
    {
        /// <summary>
        /// Определяет время суток и возвращает соответствующее приветствие
        /// </summary>
        public static string GetTimeOfDayGreeting()
        {
            var currentTime = DateTime.Now.TimeOfDay;

            if (currentTime >= new TimeSpan(10, 0, 0) && currentTime <= new TimeSpan(12, 0, 0))
                return "Доброе утро!";
            else if (currentTime >= new TimeSpan(12, 1, 0) && currentTime <= new TimeSpan(17, 0, 0))
                return "Добрый день!";
            else if (currentTime >= new TimeSpan(17, 1, 0) && currentTime <= new TimeSpan(19, 0, 0))
                return "Добрый вечер!";
            else
                return "Добро пожаловать!";
        }

        /// <summary>
        /// Проверяет, находится ли текущее время в рабочем интервале (10:00-19:00)
        /// </summary>
        public static bool IsWorkingHours()
        {
            var currentTime = DateTime.Now.TimeOfDay;
            return currentTime >= new TimeSpan(10, 0, 0) && currentTime <= new TimeSpan(19, 0, 0);
        }

        /// <summary>
        /// Формирует полное имя пользователя (Фамилия Имя Отчество)
        /// </summary>
        public static string GetFullName(string lastName, string firstName, string middleName)
        {
            string fullName = $"{lastName} {firstName}";

            if (!string.IsNullOrWhiteSpace(middleName))
            {
                fullName += $" {middleName}";
            }

            return fullName;
        }
    }
}