using pz6.Pages;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace pz6
{
    /// <summary>
    /// Главное окно приложения. Содержит Frame для навигации по страницам и кнопку возврата.
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            FrmMain.Navigate(new Autho());
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            FrmMain.GoBack();
        }

        private void FrmMain_ContentRendered(object sender, EventArgs e)
        {
            // Управляем видимостью кнопки "Назад" в зависимости от истории навигации
            btnBack.Visibility = FrmMain.CanGoBack ? Visibility.Visible : Visibility.Hidden;
        }

        /*
         Тестовые кнопки для быстрого перехода при отладке.
         Можно раскомментировать, если нужно быстро проверить страницу сотрудников.
         */
        //private void btnTestEmployees_Click(object sender, RoutedEventArgs e) { FrmMain.Navigate(new Pages.EmployeesPage()); }
        //private void btnTestAdmin_Click(object sender, RoutedEventArgs e)     { FrmMain.Navigate(new Pages.EmployeesPage()); }
    }
}