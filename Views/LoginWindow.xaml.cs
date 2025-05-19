using Cursach.BLL;
using Cursach.DAL;
using Cursach.Views;
using System.Windows;

namespace Cursach
{
    public partial class MainWindow : Window
    {
        private readonly IUserService _userService;

        public MainWindow()
        {
            InitializeComponent();
            _userService = new UserService(new UserDAL());
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_userService.ValidateUser(username, password))
            {
                string userType = _userService.GetUserType(username);
                int userId = _userService.GetUserIdByUsername(username);

                ICourseService courseService = new CourseService(new CourseDAL());
                IModuleService moduleService = new ModuleService(new ModuleDAL());
                IUserService userService = new UserService(new UserDAL());

                var courseListWindow = new CourseListWindow(userId.ToString(), userType, courseService, moduleService, userService);
                courseListWindow.Show();
                Close();
            }
            else
            {
                MessageBox.Show("Неверное имя пользователя или пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow(_userService);
            registerWindow.ShowDialog();
        }
    }
}