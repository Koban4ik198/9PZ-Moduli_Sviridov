using System;
using System.Text;

namespace pz3_.Services
{
    /// <summary>
    /// Генератор текста для CAPTCHA (только символы букв и цифр).
    /// </summary>
    internal class CaptchaGenerator
    {
        // Используем один статический Random для всего приложения
        private static readonly Random random = new Random();

        // Набор символов, из которых формируется текст капчи
        private const string Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        /// <summary>
        /// Создаёт случайную строку заданной длины для использования в CAPTCHA.
        /// </summary>
        /// <param name="length">Желаемая длина текста (должна быть > 0)</param>
        /// <returns>Случайная строка из букв и цифр</returns>
        /// <exception cref="ArgumentException">Если length ≤ 0</exception>
        public static string GenerateCaptchaText(int length)
        {
            if (length <= 0)
                throw new ArgumentException("Длина текста капчи должна быть больше нуля.");

            StringBuilder captchaText = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                // Берём случайный символ из допустимого набора
                int index = random.Next(Characters.Length);
                captchaText.Append(Characters[index]);
            }

            return captchaText.ToString();
        }
    }
}