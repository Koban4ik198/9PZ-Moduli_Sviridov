using System;
using System.Collections.Generic;

namespace pz6.Services
{
    /// <summary>
    /// Временное хранилище для 4-значных кодов восстановления и 2FA.
    /// Хранит коды в памяти с временем жизни 5 минут.
    /// </summary>
    public static class TempCodeStorage
    {
        // Вспомогательный класс вместо кортежа
        private class CodeEntry
        {
            public string Code { get; set; }
            public DateTime Expiry { get; set; }
        }

        // Явное указание типа — обязательно в C# 7.3
        private static readonly Dictionary<string, CodeEntry> _storage = new Dictionary<string, CodeEntry>();

        /// <summary>
        /// Генерирует 4-значный числовой код и сохраняет его для указанного email.
        /// Срок действия — 5 минут.
        /// </summary>
        /// <param name="email">Email пользователя</param>
        /// <returns>Сгенерированный код</returns>
        public static string GenerateAndStore(string email)
        {
            Random random = new Random();
            string code = random.Next(1000, 10000).ToString(); // 1000–9999

            var entry = new CodeEntry
            {
                Code = code,
                Expiry = DateTime.Now.AddMinutes(5)
            };

            _storage[email] = entry;
            return code;
        }

        /// <summary>
        /// Проверяет, совпадает ли введённый код с сохранённым для email.
        /// Автоматически удаляет код после проверки (одноразовое использование).
        /// </summary>
        /// <param name="email">Email пользователя</param>
        /// <param name="inputCode">Введённый пользователем код</param>
        /// <returns>True, если код верен и не истёк</returns>
        public static bool IsValid(string email, string inputCode)
        {
            if (!_storage.TryGetValue(email, out CodeEntry entry))
                return false;

            _storage.Remove(email);

            if (DateTime.Now > entry.Expiry)
                return false;

            return entry.Code == inputCode;
        }
    }
}