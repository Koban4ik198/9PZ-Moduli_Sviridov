using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using pz6.Models;

namespace pz6.Pages
{
    /// <summary>
    /// Страница со списком сотрудников (только для администраторов)
    /// </summary>
    public partial class EmployeesPage : Page
    {
        private List<Models.Employees> _allEmployees;

        public EmployeesPage(UserAuthorization currentUser = null)
        {
            InitializeComponent();

            if (currentUser != null)
            {
                if (!CheckAdminAccess(currentUser))
                {
                    MessageBox.Show("Доступ только для администраторов!");
                    NavigationService.GoBack();
                    return;
                }
            }

            LoadEmployees();
            LoadFilters();
        }

        /// <summary>Проверяет, является ли пользователь администратором</summary>
        private bool CheckAdminAccess(UserAuthorization user)
        {
            if (user == null) return false;
            if (user.Employees != null && user.Employees.Any())
            {
                var employee = user.Employees.FirstOrDefault();
                return employee.PositionID == 1;
            }
            return false;
        }

        /// <summary>Перезагружает список сотрудников при загрузке страницы</summary>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadEmployees();
        }

        /// <summary>Загружает всех сотрудников из базы вместе с должностями</summary>
        private void LoadEmployees()
        {
            var context = Helper.GetContext();
            _allEmployees = context.Employees
                .Include("Positions")
                .ToList();
            LViewEmployees.ItemsSource = _allEmployees;
        }

        /// <summary>Заполняет выпадающий список должностей для фильтра</summary>
        private void LoadFilters()
        {
            cmbFilter.Items.Clear();
            cmbFilter.Items.Add("Все");

            var context = Helper.GetContext();
            var positions = context.Positions
                .Select(p => p.PositionName)
                .Distinct()
                .ToList();

            foreach (var pos in positions)
                cmbFilter.Items.Add(pos);

            cmbFilter.SelectedIndex = 0;
        }

        /// <summary>Применяет поиск по ФИО и фильтр по должности</summary>
        private void ApplyFilters()
        {
            if (_allEmployees == null) return;

            var filtered = _allEmployees.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                string searchText = txtSearch.Text.ToLower();
                filtered = filtered.Where(emp =>
                    (emp.LastName + " " + emp.FirstName + " " + (emp.MiddleName ?? ""))
                    .ToLower().Contains(searchText));
            }

            if (cmbFilter.SelectedIndex > 0)
            {
                string selectedPosition = cmbFilter.SelectedItem.ToString();
                filtered = filtered.Where(emp =>
                    emp.Positions != null &&
                    emp.Positions.PositionName == selectedPosition);
            }

            LViewEmployees.ItemsSource = filtered.ToList();
        }

        /// <summary>Переприменяет фильтры при изменении текста поиска</summary>
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        /// <summary>Переприменяет фильтры при выборе должности</summary>
        private void cmbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        /// <summary>Переходит на страницу добавления нового сотрудника</summary>
        private void btnAddEmployee_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddEditEmployeePage(null));
        }

        /// <summary>Открывает форму редактирования при двойном клике по сотруднику</summary>
        private void LViewEmployees_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (LViewEmployees.SelectedItem is Models.Employees selectedEmployee)
            {
                NavigationService.Navigate(new AddEditEmployeePage(selectedEmployee));
            }
        }

        /// <summary>Обновляет список сотрудников из базы</summary>
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadEmployees();
        }
    }
}