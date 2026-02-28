using pz3_.Services;
using pz6.Models;
using pz6.Services;
using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace pz6.Pages
{
    public partial class Autho : Page
    {
        int click;
        private RestaurantEntities db;
        private int failedAttempts = 0;
        private DateTime? blockEndTime = null;
        private DispatcherTimer timer;
        private string _currentRecoveryEmail = null;

        public Autho()
        {
            InitializeComponent();
            click = 0;
            db = Helper.GetContext();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            UpdateUI();
        }

        /// <summary>Обрабатывает нажатие кнопки входа, проверяет данные, капчу и 2FA</summary>
        private async void btnEnter_Click(object sender, RoutedEventArgs e)
        {
            click += 1;
            if (IsBlocked())
                return;
            string login = tbLogin.Text.Trim();
            string password = tbPassword.Text.Trim();
            string hashedPassword = HashPaasword.HashPassword(password);
            var user = db.UserAuthorization
                .Include("Employees")
                .Where(x => x.Email == login && x.HashPassword == hashedPassword)
                .FirstOrDefault();
            bool needCaptcha = (click > 1);
            bool captchaValid = !needCaptcha || (needCaptcha && tbCaptcha.Text == tblCaptcha.Text);
            if (user != null && captchaValid)
            {
                failedAttempts = 0;
                click = 0;
                if (Helper.IsTwoFactorEnabled)
                {
                    string code = TempCodeStorage.GenerateAndStore(user.Email);
                    bool sent = await EmailService.SendCodeAsync(user.Email, code);
                    if (sent)
                    {
                        _currentRecoveryEmail = user.Email;
                        SwitchToCodePanel("Подтверждение входа");
                        return;
                    }
                    else
                    {
                        MessageBox.Show("Не удалось отправить код 2FA.");
                        return;
                    }
                }
                CompleteLogin(user);
            }
            else
            {
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
                CheckForBlock();
            }
        }

        /// <summary>Проверяет количество неудачных попыток и запускает блокировку при необходимости</summary>
        private void CheckForBlock()
        {
            if (failedAttempts >= 3)
            {
                BlockSystem();
            }
        }

        /// <summary>Выполняет вход в режиме гостя (без авторизации)</summary>
        private void btnEnterGuest_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Client(null, null));
        }

        /// <summary>Генерирует и отображает новую капчу</summary>
        private void GenerateCapctcha()
        {
            tbCaptcha.Visibility = Visibility.Visible;
            tblCaptcha.Visibility = Visibility.Visible;
            string capctchaText = CaptchaGenerator.GenerateCaptchaText(6);
            tblCaptcha.Text = capctchaText;
            tblCaptcha.TextDecorations = TextDecorations.Strikethrough;
        }

        /// <summary>Обновляет доступность элементов интерфейса в зависимости от состояния блокировки</summary>
        private void UpdateUI()
        {
            bool isBlocked = IsBlocked();
            tbLogin.IsEnabled = !isBlocked;
            tbPassword.IsEnabled = !isBlocked;
            tbCaptcha.IsEnabled = !isBlocked;
            btnEnter.IsEnabled = !isBlocked;
            btnEnterGuest.IsEnabled = !isBlocked;
            tblBlockTimer.Visibility = isBlocked ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>Запускает таймер блокировки на 12 секунд</summary>
        private void BlockSystem()
        {
            blockEndTime = DateTime.Now.AddSeconds(12);
            timer.Start();
            UpdateUI();
        }

        /// <summary>Проверяет, находится ли система в состоянии временной блокировки</summary>
        private bool IsBlocked()
        {
            return blockEndTime.HasValue && DateTime.Now < blockEndTime.Value;
        }

        /// <summary>Обновляет оставшееся время блокировки каждую секунду</summary>
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (blockEndTime.HasValue)
            {
                TimeSpan timeLeft = blockEndTime.Value - DateTime.Now;
                if (timeLeft.TotalSeconds <= 0)
                {
                    blockEndTime = null;
                    timer.Stop();
                    UpdateUI();
                }
                else
                {
                    tblBlockTimer.Text = $"До разблокировки: {timeLeft.Seconds} сек.";
                }
            }
        }

        /// <summary>Разрешает ввод только цифр в текстовое поле</summary>
        private void NumberValidationTextBox(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c))
                {
                    e.Handled = true;
                    break;
                }
            }
        }

        /// <summary>Переключает интерфейс на панель восстановления пароля</summary>
        private void ForgotPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            LoginPanel.Visibility = Visibility.Collapsed;
            RecoveryEmailPanel.Visibility = Visibility.Visible;
            if (txtRecoveryError != null) txtRecoveryError.Text = "";
        }

        /// <summary>Отправляет код восстановления на указанный email</summary>
        private async void SendRecoveryCode_Click(object sender, RoutedEventArgs e)
        {
            string email = tbRecoveryEmail?.Text?.Trim();
            if (string.IsNullOrEmpty(email))
            {
                if (txtRecoveryError != null) txtRecoveryError.Text = "Введите email";
                return;
            }
            var user = db.UserAuthorization.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                if (txtRecoveryError != null) txtRecoveryError.Text = "Пользователь не найден";
                return;
            }
            string code = TempCodeStorage.GenerateAndStore(email);
            bool sent = await EmailService.SendCodeAsync(email, code);
            if (!sent)
            {
                if (txtRecoveryError != null) txtRecoveryError.Text = "Ошибка отправки email";
                return;
            }
            _currentRecoveryEmail = email;
            SwitchToCodePanel("Восстановление пароля");
        }

        /// <summary>Проверяет введённый код подтверждения и переходит к следующему шагу</summary>
        private void VerifyCode_Click(object sender, RoutedEventArgs e)
        {
            string code = tbCode?.Text?.Trim();
            if (string.IsNullOrEmpty(code) || code.Length != 4 || !code.All(char.IsDigit))
            {
                if (txtCodeError != null) txtCodeError.Text = "Введите 4-значный код";
                return;
            }
            if (TempCodeStorage.IsValid(_currentRecoveryEmail, code))
            {
                if (txtCodeTitle?.Text == "Подтверждение входа")
                {
                    var user = db.UserAuthorization.First(u => u.Email == _currentRecoveryEmail);
                    CompleteLogin(user);
                }
                else
                {
                    CodePanel.Visibility = Visibility.Collapsed;
                    NewPasswordPanel.Visibility = Visibility.Visible;
                    if (txtNewPasswordError != null) txtNewPasswordError.Text = "";
                }
            }
            else
            {
                if (txtCodeError != null) txtCodeError.Text = "Неверный или просроченный код";
            }
        }

        /// <summary>Сохраняет новый пароль после успешной проверки кода</summary>
        private void SaveNewPassword_Click(object sender, RoutedEventArgs e)
        {
            string pass1 = tbNewPassword?.Text;
            string pass2 = tbConfirmPassword?.Text;
            if (string.IsNullOrEmpty(pass1) || string.IsNullOrEmpty(pass2))
            {
                if (txtNewPasswordError != null) txtNewPasswordError.Text = "Заполните все поля";
                return;
            }
            if (pass1 != pass2)
            {
                if (txtNewPasswordError != null) txtNewPasswordError.Text = "Пароли не совпадают";
                return;
            }
            var user = db.UserAuthorization.First(u => u.Email == _currentRecoveryEmail);
            user.HashPassword = HashPaasword.HashPassword(pass1);
            db.SaveChanges();
            MessageBox.Show("Пароль успешно изменён!");
            BackToLogin();
        }

        /// <summary>Переключает интерфейс на панель ввода кода подтверждения</summary>
        private void SwitchToCodePanel(string title)
        {
            RecoveryEmailPanel.Visibility = Visibility.Collapsed;
            NewPasswordPanel.Visibility = Visibility.Collapsed;
            LoginPanel.Visibility = Visibility.Collapsed;
            CodePanel.Visibility = Visibility.Visible;
            if (txtCodeTitle != null) txtCodeTitle.Text = title;
            if (txtCodeError != null) txtCodeError.Text = "";
            if (tbCode != null) tbCode.Text = "";
        }

        /// <summary>Возвращает интерфейс на панель авторизации</summary>
        private void BackToLogin()
        {
            RecoveryEmailPanel.Visibility = Visibility.Collapsed;
            CodePanel.Visibility = Visibility.Collapsed;
            NewPasswordPanel.Visibility = Visibility.Collapsed;
            LoginPanel.Visibility = Visibility.Visible;
        }

        /// <summary>Завершает процесс входа и перенаправляет на соответствующую страницу</summary>
        private void CompleteLogin(UserAuthorization user)
        {
            if (user.Employees != null && user.Employees.Any())
            {
                var employee = user.Employees.FirstOrDefault();
                if (!pz6.Helpers.TimeHelper.IsWorkingHours())
                {
                    MessageBox.Show("Доступ запрещен! Рабочее время: 10:00 - 19:00");
                    return;
                }
                if (employee.PositionID == 1)
                {
                    MessageBox.Show($"Добро пожаловать, администратор {employee.LastName}!");
                    NavigationService.Navigate(new EmployeesPage(user));
                }
                else
                {
                    MessageBox.Show($"Добро пожаловать, сотрудник {employee.LastName}!");
                    NavigationService.Navigate(new Client(user, "Сотрудник"));
                }
            }
            else
            {
                MessageBox.Show("Добро пожаловать!");
                NavigationService.Navigate(new Client(user, "Клиент"));
            }
        }

        /// <summary>Возвращает на панель входа из панели кода</summary>
        private void BackToLogin_Click(object sender, RoutedEventArgs e) => BackToLogin();

        /// <summary>Возвращает на панель входа из панели кода</summary>
        private void BackFromCode_Click(object sender, RoutedEventArgs e) => BackToLogin();

        /// <summary>Возвращает на панель входа из панели нового пароля</summary>
        private void BackFromNewPassword_Click(object sender, RoutedEventArgs e) => BackToLogin();
    }
}