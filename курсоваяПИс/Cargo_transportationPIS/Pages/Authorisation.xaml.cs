using Microsoft.Win32;
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

namespace Cargo_transportationPIS.Pages
{
    /// <summary>
    /// Логика взаимодействия для Authorisation.xaml
    /// </summary>
    public partial class Authorisation : Page
    {
        public Authorisation()
        {
            InitializeComponent();
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var userObj = GruzzPISEntities.GetContext().Пользователи.FirstOrDefault(x
                    => x.Логин == txtLogin.Text && x.Пароль == Password.Password);
                if (userObj == null)
                {
                    MessageBox.Show("Такого пользователя нет!", "Ошибка при авторизации!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Dictionary<int, (String, Page)> id2msgAndForm = new Dictionary<int, (String, Page)>{
                    {1, ("Добро пожаловать в систему, Администратор " + userObj.Имя + "!", null)},
                    {2, ("Добро пожаловать в систему, Администратор " + userObj.Имя + "!", new ManagerView())},
                    {3,  ("Добро пожаловать в систему, клиент " + userObj.Имя + "!", new ClientView())},
                    {4,  ("Добро пожаловать в систему, водитель " + userObj.Имя + "!", new DriverView())}
                };

                if (false == id2msgAndForm.ContainsKey(userObj.ID_роли)) {
                    MessageBox.Show("Данные не обнаружены!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                (String msg, Page page) = id2msgAndForm[userObj.ID_роли];

                MessageBox.Show(msg, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                AppFrame.framelMain.Navigate(page);

                GruzzPISEntities.GetContext().SaveChanges();
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Ошибка " + Ex.Message.ToString() + "Критическая работа приложения!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }

       
        private void IconPasswordN1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            IconPasswordN1.Visibility = Visibility.Hidden;
            IconPasswordN2.Visibility = Visibility.Visible;
            PasswordTextBox.Text = Password.Password;
            Password.Visibility = Visibility.Hidden;
            PasswordTextBox.Visibility = Visibility.Visible;
        }

        private void IconPasswordN2_MouseLeave(object sender, MouseEventArgs e)
        {
            IconPasswordN2.Visibility = Visibility.Hidden;
            IconPasswordN1.Visibility = Visibility.Visible;
            Password.Visibility = Visibility.Visible;
            PasswordTextBox.Visibility = Visibility.Hidden;
        }

        private void IconPasswordN2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            IconPasswordN2.Visibility = Visibility.Hidden;
            IconPasswordN1.Visibility = Visibility.Visible;
            Password.Visibility = Visibility.Visible;
            PasswordTextBox.Visibility = Visibility.Hidden;
        }
    }
}