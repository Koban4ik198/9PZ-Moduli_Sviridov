using pz3_.Services;
using pz6.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace pz6.Pages
{
    /// <summary>
    /// Логика взаимодействия для AddEditEmployeePage.xaml
    /// </summary>
    public partial class AddEditEmployeePage : Page
    {
        private Employees _currentEmployee = new Employees();
        private bool _isEditMode = false;

        public AddEditEmployeePage(Employees selectedEmployee)
        {
            InitializeComponent();

            if (selectedEmployee != null)
            {
                _currentEmployee = new Employees
                {
                    EmployeeID = selectedEmployee.EmployeeID,
                    LastName = selectedEmployee.LastName,
                    FirstName = selectedEmployee.FirstName,
                    MiddleName = selectedEmployee.MiddleName,
                    ContactPhone = selectedEmployee.ContactPhone,
                    PositionID = selectedEmployee.PositionID,
                    AutID = selectedEmployee.AutID
                };

                _isEditMode = true;
                btnDelete.Visibility = Visibility.Visible;

                // Загружаем email через ОТДЕЛЬНЫЙ контекст
                using (var tempDb = new RestaurantEntities()) // ПЕРЕИМЕНОВАЛ
                {
                    var userAuth = tempDb.UserAuthorization
                        .FirstOrDefault(u => u.ID == _currentEmployee.AutID);
                    if (userAuth != null)
                    {
                        txtEmail.Text = userAuth.Email;
                    }
                }
            }
            else
            {
                btnDelete.Visibility = Visibility.Collapsed;
            }

            DataContext = _currentEmployee;
            LoadPositions();
        }

        private void LoadPositions()
        {
            try
            {
                using (var db = new RestaurantEntities()) // ПЕРЕИМЕНОВАЛ
                {
                    var positions = db.Positions.ToList();
                    cmbPosition.ItemsSource = positions;

                    if (_isEditMode && _currentEmployee.PositionID.HasValue)
                    {
                        cmbPosition.SelectedValue = _currentEmployee.PositionID.Value;
                    }
                    else if (positions.Any())
                    {
                        cmbPosition.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки должностей: {ex.Message}");
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // ============ ВАЛИДАЦИЯ ============
            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

            // 1. Создаём объекты для валидации
            var employeeValidation = new EmployeeValidation
            {
                LastName = txtLastName.Text.Trim(),
                FirstName = txtFirstName.Text.Trim(),
                MiddleName = txtMiddleName.Text?.Trim(),
                ContactPhone = txtContactPhone.Text?.Trim(),
                PositionID = cmbPosition.SelectedValue as int?
            };

            var authValidation = new UserAuthValidation
            {
                Email = txtEmail.Text.Trim(),
                Password = txtPassword.Password
            };

            // 2. Валидируем сотрудника
            var empContext = new ValidationContext(employeeValidation);
            Validator.TryValidateObject(employeeValidation, empContext, validationResults, true);

            // 3. Валидируем авторизацию (только для новых или при смене пароля)
            if (!_isEditMode || !string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                var authContext = new ValidationContext(authValidation);
                Validator.TryValidateObject(authValidation, authContext, validationResults, true);
            }

            // 4. Проверяем должность отдельно (ComboBox)
            if (cmbPosition.SelectedItem == null)
            {
                validationResults.Add(new System.ComponentModel.DataAnnotations.ValidationResult("Выберите должность", new[] { "PositionID" }));
            }

            // 5. Если есть ошибки - показываем их
            if (validationResults.Any())
            {
                string errorMessage = "Обнаружены ошибки:\n\n" +
                    string.Join("\n", validationResults.Select(vr => $"• {vr.ErrorMessage}"));

                MessageBox.Show(errorMessage, "Ошибка валидации",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // ============ ОСТАЛЬНАЯ ЛОГИКА ============
            try
            {
                if (!_isEditMode)
                {
                    // ============ ДОБАВЛЕНИЕ НОВОГО СОТРУДНИКА ============
                    using (var db = new RestaurantEntities())
                    {
                        // 1. Проверяем, нет ли уже такого email
                        var existingEmail = db.UserAuthorization
                            .FirstOrDefault(u => u.Email == txtEmail.Text.Trim());

                        if (existingEmail != null)
                        {
                            MessageBox.Show("Такой email уже используется!");
                            return;
                        }

                        // 2. Создаём запись авторизации
                        var newUserAuth = new UserAuthorization
                        {
                            Email = txtEmail.Text.Trim(),
                            HashPassword = HashPaasword.HashPassword(txtPassword.Password)
                        };

                        db.UserAuthorization.Add(newUserAuth);
                        db.SaveChanges(); // Получаем ID

                        // 3. Создаём сотрудника
                        var newEmployee = new Employees
                        {
                            LastName = txtLastName.Text.Trim(),
                            FirstName = txtFirstName.Text.Trim(),
                            MiddleName = txtMiddleName.Text?.Trim(),
                            ContactPhone = txtContactPhone.Text?.Trim(),
                            PositionID = (int?)cmbPosition.SelectedValue,
                            AutID = newUserAuth.ID
                        };

                        db.Employees.Add(newEmployee);
                        db.SaveChanges();

                        MessageBox.Show("Сотрудник добавлен успешно!\nЛогин: " + txtEmail.Text);
                    }
                }
                else
                {
                    // ============ РЕДАКТИРОВАНИЕ СУЩЕСТВУЮЩЕГО ============
                    using (var db = new RestaurantEntities())
                    {
                        var existing = db.Employees
                            .Include("Positions")
                            .FirstOrDefault(emp => emp.EmployeeID == _currentEmployee.EmployeeID);

                        if (existing != null)
                        {
                            // Обновляем поля сотрудника
                            existing.LastName = txtLastName.Text.Trim();
                            existing.FirstName = txtFirstName.Text.Trim();
                            existing.MiddleName = txtMiddleName.Text?.Trim();
                            existing.ContactPhone = txtContactPhone.Text?.Trim();
                            existing.PositionID = (int?)cmbPosition.SelectedValue;

                            // Обновляем связанную должность
                            if (existing.PositionID.HasValue)
                            {
                                existing.Positions = db.Positions
                                    .FirstOrDefault(p => p.PositionID == existing.PositionID.Value);
                            }

                            // Обновляем email/пароль если изменили
                            if (!string.IsNullOrWhiteSpace(txtPassword.Password))
                            {
                                var userAuth = db.UserAuthorization
                                    .FirstOrDefault(u => u.ID == existing.AutID);

                                if (userAuth != null)
                                {
                                    userAuth.Email = txtEmail.Text.Trim();
                                    userAuth.HashPassword = HashPaasword.HashPassword(txtPassword.Password);
                                }
                            }

                            db.SaveChanges();
                            MessageBox.Show("Данные обновлены успешно!");
                        }
                        else
                        {
                            MessageBox.Show("Сотрудник не найден в базе");
                            return;
                        }
                    }
                }

                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}\n{ex.InnerException?.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Удалить этого сотрудника?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                using (var context = new RestaurantEntities()) // НОВЫЙ контекст
                {
                    try
                    {
                        var employeeToDelete = context.Employees.Find(_currentEmployee.EmployeeID);
                        if (employeeToDelete != null)
                        {
                            context.Employees.Remove(employeeToDelete);
                            context.SaveChanges();
                            MessageBox.Show("Сотрудник удален!");
                            NavigationService.GoBack();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}");
                    }
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}