using pz6.Helpers;
using pz6.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace pz6.Pages
{
    public partial class Client : Page
    {
        private UserAuthorization _user;
        private string _role;

        public Client(UserAuthorization user, string role)
        {
            InitializeComponent();
            _user = user;
            _role = role;

            DisplayGreeting();
        }

        private void DisplayGreeting()
        {
            if (_user != null)
            {
                // Получаем приветствие по времени суток
                string greeting = TimeHelper.GetTimeOfDayGreeting();
                txtGreeting.Text = greeting + "!";

                // Определяем, кто вошел: клиент или сотрудник
                string fullName = "";

                // Проверяем, есть ли запись в таблице Employees
                using (var db = new RestaurantEntities())
                {
                    var employee = db.Employees
                        .Where(e => e.AutID == _user.ID)
                        .FirstOrDefault();

                    if (employee != null)
                    {
                        // Это сотрудник - выводим ФИО
                        fullName = $"{employee.LastName} {employee.FirstName} {employee.MiddleName}".Trim();
                    }
                    else
                    {
                        // Это клиент - проверяем таблицу Clients
                        var client = db.Clients
                            .Where(c => c.AutID == _user.ID)
                            .FirstOrDefault();

                        if (client != null)
                        {
                            fullName = $"{client.LastName} {client.FirstName} {client.MiddleName}".Trim();
                        }
                        else
                        {
                            fullName = _user.Email; // Запасной вариант
                        }
                    }
                }

                txtUserName.Text = fullName;
            }
            else
            {
                // Гость
                txtGreeting.Text = "Добро пожаловать!";
                txtUserName.Text = "Гость";
            }
        }
    }
}