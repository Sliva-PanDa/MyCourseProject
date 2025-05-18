using Cursach.BLL;
using Cursach.DAL;
using System;
using System.Windows;

namespace Cursach
{
    public partial class MainWindow : Window
    {
        private readonly IUserService _userService;

        public MainWindow(IUserService userService)
        {
            InitializeComponent();
            _userService = userService;
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string username = txtUsername.Text;
                string password = txtPassword.Password;

                if (_userService.ValidateUser(username, password))
                {
                    string userType = _userService.GetUserType(username);
                    if (userType == null)
                    {
                        MessageBox.Show("Не удалось определить тип пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    CourseListWindow courseWindow = new CourseListWindow(username, userType, new CourseService(new CourseDAL()), new ModuleService(new ModuleDAL()), new UserService(new UserDAL()));
                    courseWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Неверное имя пользователя или пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}