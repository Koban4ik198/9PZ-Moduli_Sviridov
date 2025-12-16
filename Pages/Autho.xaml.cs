using pz3_.Services;
using pz6.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Data.Entity;
namespace pz6.Pages
{
    public partial class Autho : Page
    {
        int click;
        private RestaurantEntities db;
        private int failedAttempts = 0;
        private DateTime? blockEndTime = null;
        private DispatcherTimer timer;

        public Autho()
        {
            InitializeComponent();
            click = 0;
            db = Helper.GetContext(); // Инициализация контекста

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
        }

        private void btnEnter_Click(object sender, RoutedEventArgs e)
        {
            click += 1;

            if (IsBlocked())
                return;

            string login = tbLogin.Text.Trim();
            string password = tbPassword.Text.Trim();

            // Если используете хэширование:
            string hashedPassword = HashPaasword.HashPassword(password);

            var user = db.UserAuthorization
                .Include("Employees") // Добавляем загрузку связанных данных
                .Where(x => x.Email == login && x.HashPassword == hashedPassword)
                .FirstOrDefault();

            // Проверяем капчу только со второй попытки
            bool needCaptcha = (click > 1);
            bool captchaValid = !needCaptcha || (needCaptcha && tbCaptcha.Text == tblCaptcha.Text);

            if (user != null && captchaValid)
            {
                // Успешный вход - сбрасываем счетчик ошибок
                failedAttempts = 0;
                click = 0; // Сбрасываем счетчик попыток

                // Проверяем, является ли пользователь сотрудником
                if (user.Employees != null && user.Employees.Any())
                {
                    var employee = user.Employees.FirstOrDefault();

                    // Проверка рабочего времени для сотрудников
                    if (!pz6.Helpers.TimeHelper.IsWorkingHours())
                    {
                        MessageBox.Show("Доступ запрещен! Рабочее время: 10:00 - 19:00");
                        return;
                    }

                    // Проверка роли
                    if (employee.PositionID == 1) // 1 = Администратор
                    {
                        MessageBox.Show($"Добро пожаловать, администратор {employee.LastName}!");
                        NavigationService.Navigate(new EmployeesPage(user)); // Передаём пользователя
                        return;
                    }
                    else
                    {
                        MessageBox.Show($"Добро пожаловать, сотрудник {employee.LastName}!");
                        NavigationService.Navigate(new Client(user, "Сотрудник"));
                        return;
                    }
                }
                else
                {
                    // Обычный клиент (не сотрудник)
                    MessageBox.Show("Добро пожаловать!");
                    NavigationService.Navigate(new Client(user, "Клиент"));
                    return;
                }
            }
            else
            {
                // Неправильные данные или капча
                failedAttempts++;

                if (user == null)
                {
                    MessageBox.Show("Неверный логин или пароль!");
                }
                else if (needCaptcha && !captchaValid)
                {
                    MessageBox.Show("Неверная капча!");
                }

                tbPassword.Clear();
                tbCaptcha.Clear();
                GenerateCapctcha();

                // Проверяем, нужно ли блокировать
                CheckForBlock();
            }
        }

        private void CheckForBlock()
        {
            // Блокируем систему после 3-х неудачных попыток
            if (failedAttempts >= 3)
            {
                BlockSystem();
            }
        }

        private void btnEnterGuest_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Client(null, null));
        }

        private void GenerateCapctcha()
        {
            tbCaptcha.Visibility = Visibility.Visible;
            tblCaptcha.Visibility = Visibility.Visible;

            string capctchaText = CaptchaGenerator.GenerateCaptchaText(6);
            tblCaptcha.Text = capctchaText;
            tblCaptcha.TextDecorations = TextDecorations.Strikethrough;
        }

        private void UpdateUI()
        {
            bool isBlocked = IsBlocked();

            // Управление свойством IsEnabled всех интерактивных элементов
            tbLogin.IsEnabled = !isBlocked;
            tbPassword.IsEnabled = !isBlocked;
            tbCaptcha.IsEnabled = !isBlocked;
            btnEnter.IsEnabled = !isBlocked;
            btnEnterGuest.IsEnabled = !isBlocked;

            // Управление видимостью таймера
            tblBlockTimer.Visibility = isBlocked ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BlockSystem()
        {
            blockEndTime = DateTime.Now.AddSeconds(12);     // Устанавливаем время разблокировки (текущее время + 10 сек)
            timer.Start();                                  // Запускаем таймер (IsEnabled = true)
            UpdateUI();                                     // Блокируем UI элементы
        }

        private bool IsBlocked()
        {
            // Возвращает true если: время блокировки установлено И текущее время меньше времени разблокировки
            return blockEndTime.HasValue && DateTime.Now < blockEndTime.Value;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Этот метод вызывается АВТОМАТИЧЕСКИ каждую секунду, пока timer.IsEnabled == true

            if (blockEndTime.HasValue)                                   // Проверяем, установлено ли время блокировки
            {
                TimeSpan timeLeft = blockEndTime.Value - DateTime.Now;   // Вычисляем оставшееся время

                if (timeLeft.TotalSeconds <= 0)                         // Если время вышло
                {
                    // СБРОС БЛОКИРОВКИ
                    blockEndTime = null;                                // Обнуляем время блокировки
                    timer.Stop();                                       // Останавливаем таймер (IsEnabled = false)
                    UpdateUI();                                         // Обновляем состояние UI элементов
                }
                else
                {
                    // ОБНОВЛЕНИЕ ОТОБРАЖЕНИЯ
                    tblBlockTimer.Text = $"До разблокировки: {timeLeft.Seconds} сек.";
                }
            }
        }
    }
}