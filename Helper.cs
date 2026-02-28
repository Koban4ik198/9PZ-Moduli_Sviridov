using pz6.Models;

namespace pz6
{
    /// <summary>
    /// Вспомогательный класс с общими утилитами приложения.
    /// Содержит доступ к контексту базы данных и глобальные настройки.
    /// </summary>
    internal class Helper
    {
        // Статическое поле для хранения единственного экземпляра контекста
        private static RestaurantEntities _context;

        /// <summary>
        /// Возвращает единый экземпляр контекста Entity Framework (паттерн Singleton).
        /// Создаёт контекст только при первом обращении.
        /// </summary>
        public static RestaurantEntities GetContext()
        {
            // Ленивая инициализация — создаём контекст только если его ещё нет
            if (_context == null)
            {
                _context = new RestaurantEntities();
            }
            return _context;
        }

        // Глобальная настройка — включена ли двухфакторная аутентификация во всём приложении
        public static bool IsTwoFactorEnabled = true;
    }
}