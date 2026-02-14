using System;
using System.Collections.Generic;
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
using pz6.Models;

namespace pz6.Pages
{
    /// <summary>
    /// Логика взаимодействия для EmployeesPage.xaml
    /// </summary>
    public partial class EmployeesPage : Page
    {
        private List<Models.Employees> _allEmployees;

        public EmployeesPage(UserAuthorization currentUser = null)
        {
            InitializeComponent();

            // Если передали пользователя - проверяем права
            if (currentUser != null)
            {
                if (!CheckAdminAccess(currentUser))
                {
                    MessageBox.Show("Доступ только для администраторов!");
                    NavigationService.GoBack();
                    return;
                }
            }
            else
            {
            }

            LoadEmployees();
            LoadFilters();
        }

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

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadEmployees();
        }

        private void LoadEmployees()
        {
            var context = Helper.GetContext();
            _allEmployees = context.Employees
                .Include("Positions")
                .ToList();
            LViewEmployees.ItemsSource = _allEmployees; 
        }

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

        private void ApplyFilters()
        {
            if (_allEmployees == null) return;

            var filtered = _allEmployees.AsEnumerable();

            // Поиск по ФИО
            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                string searchText = txtSearch.Text.ToLower();
                filtered = filtered.Where(emp =>
                    (emp.LastName + " " + emp.FirstName + " " + (emp.MiddleName ?? ""))
                    .ToLower().Contains(searchText));
            }

            // Фильтр по должности
            if (cmbFilter.SelectedIndex > 0)
            {
                string selectedPosition = cmbFilter.SelectedItem.ToString();
                filtered = filtered.Where(emp =>
                    emp.Positions != null &&
                    emp.Positions.PositionName == selectedPosition);
            }

            LViewEmployees.ItemsSource = filtered.ToList();
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void cmbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void btnAddEmployee_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddEditEmployeePage(null));
        }

        private void LViewEmployees_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (LViewEmployees.SelectedItem is Models.Employees selectedEmployee)
            {
                NavigationService.Navigate(new AddEditEmployeePage(selectedEmployee));
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadEmployees();
        }
    }
}
