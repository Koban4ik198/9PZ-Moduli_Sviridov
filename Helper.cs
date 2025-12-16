using pz6.Models;

namespace pz6
{
    internal class Helper
    {
        private static RestaurantEntities _context;

        public static RestaurantEntities GetContext()
        {
            if (_context == null)
            {
                _context = new RestaurantEntities();
            }
            return _context;
        }
    }
}
