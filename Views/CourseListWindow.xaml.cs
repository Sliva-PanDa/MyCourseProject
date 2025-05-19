using Cursach.BLL;
using Cursach.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Cursach
{
    public partial class CourseListWindow : Window
    {
        private readonly int _userId;
        private readonly string _userType;
        private readonly ICourseService _courseService;
        private readonly IModuleService _moduleService;
        private readonly IUserService _userService;
        private List<Course> _allCourses;
        private bool _themesLoaded = false;

        public CourseListWindow(string userId, string userType, ICourseService courseService, IModuleService moduleService, IUserService userService)
        {
            InitializeComponent();
            _userId = int.Parse(userId);
            _userType = userType;
            _courseService = courseService;
            _moduleService = moduleService;
            _userService = userService;
            Console.WriteLine($"CourseListWindow: UserID={_userId}, UserType={_userType}");
            LoadCourses();
            ConfigureTabs();
            LoadUserCourses();
        }

        private void ConfigureTabs()
        {
            if (_userType != "administrator")
            {
                addCourseTab.Visibility = Visibility.Collapsed;
                editCourseTab.Visibility = Visibility.Collapsed;
            }
            else
            {
                LoadInstructors();
                progressTab.Visibility = Visibility.Collapsed;
                myCoursesTab.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadCourses()
        {
            _allCourses = _courseService.GetAllCourses();
            dgCourses.ItemsSource = _allCourses;
            Console.WriteLine("Загруженные курсы:");
            foreach (var course in _allCourses)
            {
                Console.WriteLine($"CourseID={course.CourseID}, Title={course.Title}, Theme={course.Theme}");
            }
            var availableCourses = _allCourses.Where(c => !IsEnrolled(c.CourseID)).ToList();
            if (dgAvailableCourses != null)
            {
                dgAvailableCourses.ItemsSource = availableCourses;
            }
            Console.WriteLine($"Всего курсов: {_allCourses.Count}");
            Console.WriteLine($"Доступных курсов: {availableCourses.Count}");
            LoadMyCourses();
        }

        private void LoadInstructors()
        {
            var instructors = _userService.GetAllUsers().Where(u => u.UserType == "instructor").ToList();
            cmbInstructor.ItemsSource = instructors;
            cmbEditInstructor.ItemsSource = instructors;
        }

        private void LoadThemes()
        {
            var themes = _allCourses != null && _allCourses.Any()
                ? _allCourses.Select(c => c.Theme).Distinct().ToList()
                : new List<string>();
            themes.Insert(0, "Все темы");
            if (cmbThemeFilter != null)
            {
                cmbThemeFilter.ItemsSource = themes;
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabControl.SelectedItem == availableCoursesTab && !_themesLoaded)
            {
                LoadThemes();
                _themesLoaded = true;
            }
        }

        private void LoadMyCourses()
        {
            using (MySqlConnection conn = new MySqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT c.Title AS CourseTitle, e.EnrollmentDate, " +
                    "CASE WHEN COUNT(ms.StatusID) = (SELECT COUNT(*) FROM module m WHERE m.CourseID = c.CourseID) " +
                    "THEN 'Завершено' ELSE 'В процессе' END AS CompletionStatus " +
                    "FROM enrollment e " +
                    "JOIN course c ON e.CourseID = c.CourseID " +
                    "LEFT JOIN moduleStatus ms ON e.EnrollmentID = ms.EnrollmentID " +
                    "WHERE e.UserID = @userId " +
                    "GROUP BY c.CourseID, c.Title, e.EnrollmentDate", conn);
                cmd.Parameters.AddWithValue("@userId", _userId);
                var myCourses = new List<MyCourse>();
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        myCourses.Add(new MyCourse
                        {
                            CourseTitle = reader.GetString("CourseTitle"),
                            EnrollmentDate = reader.GetDateTime("EnrollmentDate"),
                            CompletionStatus = reader.GetString("CompletionStatus")
                        });
                    }
                }
                dgMyCourses.ItemsSource = myCourses;
            }
        }

        private void LoadUserCourses()
        {
            List<Course> userCourses = new List<Course>();
            using (MySqlConnection conn = new MySqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT CourseID FROM enrollment WHERE UserID = @userId", conn);
                cmd.Parameters.AddWithValue("@userId", _userId);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int courseId = reader.GetInt32("CourseID");
                        var course = _courseService.GetCourseById(courseId);
                        if (course != null)
                        {
                            userCourses.Add(course);
                        }
                    }
                }
            }
            if (cmbCourses != null)
            {
                cmbCourses.ItemsSource = userCourses;
            }
        }

        private bool IsEnrolled(int courseId)
        {
            using (MySqlConnection conn = new MySqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM enrollment WHERE UserID = @userId AND CourseID = @courseId", conn);
                cmd.Parameters.Add("@userId", MySqlDbType.Int32).Value = _userId;
                cmd.Parameters.Add("@courseId", MySqlDbType.Int32).Value = courseId;
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                Console.WriteLine($"IsEnrolled: UserID={_userId}, CourseID={courseId}, Count={count}, ConnectionString={conn.ConnectionString}");
                return count > 0;
            }
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = txtSearch.Text.ToLower();
            dgCourses.ItemsSource = _allCourses?.Where(c => c.Title.ToLower().Contains(searchText) || c.Theme.ToLower().Contains(searchText)).ToList() ?? new List<Course>();
        }

        private void AvailableSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = txtAvailableSearch.Text.ToLower();
            var filteredCourses = _allCourses?.Where(c => !IsEnrolled(c.CourseID) && (c.Title.ToLower().Contains(searchText) || c.Theme.ToLower().Contains(searchText))).ToList() ?? new List<Course>();
            if (dgAvailableCourses != null)
            {
                dgAvailableCourses.ItemsSource = filteredCourses;
            }
        }

        private void ThemeFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedTheme = (cmbThemeFilter.SelectedItem as string) ?? "Все темы";
            var filteredCourses = _allCourses?.Where(c => !IsEnrolled(c.CourseID) && (selectedTheme == "Все темы" || c.Theme == selectedTheme)).ToList() ?? new List<Course>();
            if (dgAvailableCourses != null)
            {
                dgAvailableCourses.ItemsSource = filteredCourses;
            }
        }

        private void BtnSortAsc_Click(object sender, RoutedEventArgs e)
        {
            dgCourses.ItemsSource = _allCourses?.OrderBy(c => c.Price).ToList() ?? new List<Course>();
        }

        private void BtnSortDesc_Click(object sender, RoutedEventArgs e)
        {
            dgCourses.ItemsSource = _allCourses?.OrderByDescending(c => c.Price).ToList() ?? new List<Course>();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadCourses();
        }

        private void dgCourses_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgCourses.SelectedItem is Course selectedCourse)
            {
                btnEditCourse.IsEnabled = true;
                btnManageModules.IsEnabled = true;
            }
            else
            {
                btnEditCourse.IsEnabled = false;
                btnManageModules.IsEnabled = false;
            }
        }

        private void BtnEditCourse_Click(object sender, RoutedEventArgs e)
        {
            if (dgCourses.SelectedItem is Course course)
            {
                txtEditTitle.Text = course.Title;
                txtEditDescription.Text = course.Description;
                txtEditTheme.Text = course.Theme;
                txtEditPrice.Text = course.Price.ToString();
                cmbEditInstructor.SelectedValue = course.InstructorID;
                tabControl.SelectedItem = editCourseTab;
            }
        }

        private void BtnManageModules_Click(object sender, RoutedEventArgs e)
        {
            if (dgCourses.SelectedItem is Course course)
            {
                MessageBox.Show($"Управление модулями для курса: {course.Title}", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void PriceTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !IsNumericOrDecimal(e.Text);
        }

        private bool IsNumericOrDecimal(string text)
        {
            return decimal.TryParse(text, out _);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var course = new Course
                {
                    Title = txtTitle.Text,
                    Description = txtDescription.Text,
                    Theme = txtTheme.Text,
                    Price = decimal.Parse(txtPrice.Text),
                    InstructorID = (int)cmbInstructor.SelectedValue
                };
                _courseService.AddCourse(course);
                LoadCourses();
                tabControl.SelectedItem = null;
                MessageBox.Show("Курс добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSaveEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgCourses.SelectedItem is Course course)
                {
                    course.Title = txtEditTitle.Text;
                    course.Description = txtEditDescription.Text;
                    course.Theme = txtEditTheme.Text;
                    course.Price = decimal.Parse(txtEditPrice.Text);
                    course.InstructorID = (int)cmbEditInstructor.SelectedValue;
                    _courseService.UpdateCourse(course);
                    LoadCourses();
                    tabControl.SelectedItem = null;
                    MessageBox.Show("Курс обновлён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEnroll_Click(object sender, RoutedEventArgs e)
        {
            if (dgAvailableCourses != null && dgAvailableCourses.SelectedItem is Course course)
            {
                using (MySqlConnection conn = new MySqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("INSERT INTO enrollment (UserID, CourseID, EnrollmentDate) VALUES (@userId, @courseId, @date); SELECT LAST_INSERT_ID();", conn);
                    cmd.Parameters.AddWithValue("@userId", _userId);
                    cmd.Parameters.AddWithValue("@courseId", course.CourseID);
                    cmd.Parameters.AddWithValue("@date", DateTime.Now);
                    long enrollmentId = Convert.ToInt64(cmd.ExecuteScalar());

                    var modules = _moduleService.GetModulesByCourseId(course.CourseID);
                    foreach (var module in modules)
                    {
                        cmd = new MySqlCommand("INSERT INTO moduleStatus (EnrollmentID, ModuleID, Status) VALUES (@enrollmentId, @moduleId, @status)", conn);
                        cmd.Parameters.AddWithValue("@enrollmentId", enrollmentId);
                        cmd.Parameters.AddWithValue("@moduleId", module.ModuleID);
                        cmd.Parameters.AddWithValue("@status", "NotStarted");
                        cmd.ExecuteNonQuery();
                    }
                }
                LoadCourses();
                LoadUserCourses();
                MessageBox.Show($"Вы записаны на курс: {course.Title}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnManageProgress_Click(object sender, RoutedEventArgs e)
        {
            if (dgMyCourses.SelectedItem is MyCourse myCourse)
            {
                var course = _allCourses.FirstOrDefault(c => c.Title == myCourse.CourseTitle);
                if (course != null)
                {
                    LoadProgress(course.CourseID);
                    tabControl.SelectedItem = progressTab;
                }
            }
        }

        private void CmbCourses_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbCourses.SelectedItem is Course selectedCourse)
            {
                LoadProgress(selectedCourse.CourseID);
            }
        }

        private void LoadProgress(int courseId)
        {
            List<ModuleProgress> progress = new List<ModuleProgress>();
            int totalModules = 0;
            int completedModules = 0;

            using (MySqlConnection conn = new MySqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT m.ModuleID, m.Title AS ModuleTitle, ms.Status, ms.CompletionDate " +
                    "FROM module m " +
                    "LEFT JOIN moduleStatus ms ON m.ModuleID = ms.ModuleID AND ms.EnrollmentID = (SELECT EnrollmentID FROM enrollment WHERE UserID = @userId AND CourseID = @courseId) " +
                    "WHERE m.CourseID = @courseId", conn);
                cmd.Parameters.AddWithValue("@userId", _userId);
                cmd.Parameters.AddWithValue("@courseId", courseId);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        totalModules++;
                        var status = reader.IsDBNull(reader.GetOrdinal("Status")) ? "NotStarted" : reader.GetString("Status");
                        progress.Add(new ModuleProgress
                        {
                            ModuleTitle = reader.GetString("ModuleTitle"),
                            Status = status,
                            CompletionDate = reader.IsDBNull(reader.GetOrdinal("CompletionDate")) ? (DateTime?)null : reader.GetDateTime("CompletionDate")
                        });
                        if (status == "Completed")
                            completedModules++;
                    }
                }
            }

            dgProgress.ItemsSource = progress;
            lblStatsTotal.Text = $"Всего модулей: {totalModules}";
            lblStatsCompleted.Text = $"Пройдено: {completedModules}";
            lblStatsPercentage.Text = $"Завершено: {(totalModules > 0 ? Math.Round((double)completedModules / totalModules * 100, 0) : 0)}%";
        }

        private void BtnMarkCompleted_Click(object sender, RoutedEventArgs e)
        {
            if (dgProgress.SelectedItem is ModuleProgress module)
            {
                UpdateModuleStatus(module.ModuleTitle, "Completed");
            }
        }

        private void BtnMarkInProgress_Click(object sender, RoutedEventArgs e)
        {
            if (dgProgress.SelectedItem is ModuleProgress module)
            {
                UpdateModuleStatus(module.ModuleTitle, "InProgress");
            }
        }

        private void UpdateModuleStatus(string moduleTitle, string status)
        {
            using (MySqlConnection conn = new MySqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(
                    "UPDATE moduleStatus ms " +
                    "JOIN module m ON ms.ModuleID = m.ModuleID " +
                    "JOIN enrollment e ON ms.EnrollmentID = e.EnrollmentID " +
                    "SET ms.Status = @status, ms.CompletionDate = @date " +
                    "WHERE e.UserID = @userId AND m.Title = @moduleTitle AND e.CourseID = (SELECT CourseID FROM module WHERE ModuleID = m.ModuleID)", conn);
                cmd.Parameters.AddWithValue("@status", status);
                cmd.Parameters.AddWithValue("@date", status == "Completed" ? DateTime.Now : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@userId", _userId);
                cmd.Parameters.AddWithValue("@moduleTitle", moduleTitle);
                cmd.ExecuteNonQuery();
            }
            if (dgMyCourses.SelectedItem is MyCourse myCourse)
            {
                var course = _allCourses.FirstOrDefault(c => c.Title == myCourse.CourseTitle);
                if (course != null)
                    LoadProgress(course.CourseID);
            }
        }

        private void BtnBackToMyCourses_Click(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedItem = myCoursesTab;
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new MainWindow();
            loginWindow.Show();
            Close();
        }
    }

    public class MyCourse
    {
        public string CourseTitle { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public string CompletionStatus { get; set; }
    }

    public class ModuleProgress
    {
        public string ModuleTitle { get; set; }
        public string Status { get; set; }
        public DateTime? CompletionDate { get; set; }
    }
}