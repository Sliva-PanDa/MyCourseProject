using Cursach.BLL;
using Cursach.DAL;
using Cursach.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace Cursach
{
    public partial class CourseListWindow : Window
    {
        private readonly ICourseService _courseService;
        private readonly IModuleService _moduleService;
        private readonly IUserService _userService;
        private string _username;
        private string _userType;
        private int _userId;
        private int _selectedCourseId = -1;
        private int _selectedCourseIdForProgress;
        private int _enrollmentIdForProgress;

        public CourseListWindow(string username, string userType, ICourseService courseService, IModuleService moduleService, IUserService userService)
        {
            InitializeComponent();
            _username = username;
            _userType = userType;
            _courseService = courseService;
            _moduleService = moduleService;
            _userService = userService;
            this.Title = $"Курсы - {_username} ({_userType})";
            LoadUserId();
            LoadCourses();
            LoadInstructors();
            ConfigureTabs();
            LoadAvailableCourses();
            LoadMyCourses();
            LoadThemes();
        }

        private void LoadUserId() 
        {
            _userId = _userService.GetUserIdByUsername(_username);
        }

        private void ConfigureTabs() 
        {
            if (_userType == "administrator")
            {
                availableCoursesTab.Visibility = Visibility.Collapsed;
                myCoursesTab.Visibility = Visibility.Collapsed;
                progressTab.Visibility = Visibility.Collapsed;
            }
            else
            {
                addCourseTab.Visibility = Visibility.Collapsed;
                editCourseTab.Visibility = Visibility.Collapsed;
                btnEditCourse.Visibility = Visibility.Collapsed;
                btnManageModules.Visibility = Visibility.Collapsed;
                progressTab.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadCourses() 
        {
            try
            {
                dgCourses.ItemsSource = null;
                var courses = _courseService.GetAllCourses();
                for (int i = 0; i < courses.Count; i++)
                {
                    courses[i].DisplayPrice = courses[i].Price.ToString("F2");
                }
                dgCourses.ItemsSource = courses;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки курсов: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadInstructors() 
        {
            try
            {
                var instructors = _userService.GetInstructors();
                var instructorsAdd = new List<User>();
                for (int i = 0; i < instructors.Count; i++)
                {
                    instructorsAdd.Add(instructors[i]);
                }
                instructorsAdd.Add(new User { UserID = 0, UserName = "(Нет инструктора)" });
                cmbInstructor.ItemsSource = instructorsAdd;
                cmbInstructor.DisplayMemberPath = "UserName";
                cmbInstructor.SelectedValuePath = "UserID";
                cmbInstructor.SelectedIndex = 0;

                var instructorsEdit = new List<User>();
                for (int i = 0; i < instructors.Count; i++)
                {
                    instructorsEdit.Add(instructors[i]);
                }
                instructorsEdit.Add(new User { UserID = 0, UserName = "(Нет инструктора)" });
                cmbEditInstructor.ItemsSource = instructorsEdit;
                cmbEditInstructor.DisplayMemberPath = "UserName";
                cmbEditInstructor.SelectedValuePath = "UserID";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки инструкторов: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e) 
        {
            try
            {
                string searchTerm = txtSearch.Text;
                dgCourses.ItemsSource = null;
                var courses = string.IsNullOrWhiteSpace(searchTerm) ? _courseService.GetAllCourses() : _courseService.SearchCourses(searchTerm);
                for (int i = 0; i < courses.Count; i++)
                {
                    courses[i].DisplayPrice = courses[i].Price.ToString("F2");
                }
                dgCourses.ItemsSource = courses;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка поиска курсов: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e) 
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text) || string.IsNullOrWhiteSpace(txtTheme.Text) || string.IsNullOrWhiteSpace(txtPrice.Text))
            {
                MessageBox.Show("Пожалуйста, заполните Название, Тему и Цену курса.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtPrice.Text.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal price) || price < 0)
            {
                MessageBox.Show("Цена должна быть неотрицательным числом.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int? instructorId = (int?)cmbInstructor.SelectedValue;

            try
            {
                _courseService.AddCourse(new Course
                {
                    Title = txtTitle.Text,
                    Description = txtDescription.Text,
                    Theme = txtTheme.Text,
                    Price = price,
                    InstructorID = instructorId == 0 ? null : instructorId
                });
                MessageBox.Show("Курс успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadCourses();
                ClearAddForm();
                tabControl.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении курса: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearAddForm() 
        {
            txtTitle.Text = string.Empty;
            txtDescription.Text = string.Empty;
            txtTheme.Text = string.Empty;
            txtPrice.Text = string.Empty;
            cmbInstructor.SelectedIndex = 0;
        }

        private void BtnEditCourse_Click(object sender, RoutedEventArgs e) 
        {
            if (dgCourses.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите курс для редактирования.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedCourse = (Course)dgCourses.SelectedItem;
            try
            {
                _selectedCourseId = selectedCourse.CourseID;
                txtEditTitle.Text = selectedCourse.Title;
                txtEditDescription.Text = selectedCourse.Description ?? string.Empty;
                txtEditTheme.Text = selectedCourse.Theme;
                txtEditPrice.Text = selectedCourse.Price.ToString(System.Globalization.CultureInfo.InvariantCulture);
                cmbEditInstructor.SelectedValue = selectedCourse.InstructorID ?? 0;
                tabControl.SelectedIndex = 2;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных для редактирования: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                _selectedCourseId = -1;
            }
        }

        private void BtnSaveEdit_Click(object sender, RoutedEventArgs e) 
        {
            if (_selectedCourseId == -1)
            {
                MessageBox.Show("Не выбран курс для редактирования.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtEditTitle.Text) || string.IsNullOrWhiteSpace(txtEditTheme.Text) || string.IsNullOrWhiteSpace(txtEditPrice.Text))
            {
                MessageBox.Show("Пожалуйста, заполните Название, Тему и Цену курса.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtEditPrice.Text.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal price) || price < 0)
            {
                MessageBox.Show("Цена должна быть неотрицательным числом.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int? instructorId = (int?)cmbEditInstructor.SelectedValue;

            try
            {
                _courseService.UpdateCourse(new Course
                {
                    CourseID = _selectedCourseId,
                    Title = txtEditTitle.Text,
                    Description = txtEditDescription.Text,
                    Theme = txtEditTheme.Text,
                    Price = price,
                    InstructorID = instructorId == 0 ? null : instructorId
                });
                MessageBox.Show("Курс успешно обновлён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadCourses();
                tabControl.SelectedIndex = 0;
                _selectedCourseId = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении курса: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e) 
        {
            txtSearch.Text = string.Empty;
            LoadCourses();
        }

        private void BtnSortAsc_Click(object sender, RoutedEventArgs e) 
        {
            try
            {
                dgCourses.ItemsSource = null;
                var courses = _courseService.GetCoursesSortedByPrice(true);
                for (int i = 0; i < courses.Count; i++)
                {
                    courses[i].DisplayPrice = courses[i].Price.ToString("F2");
                }
                dgCourses.ItemsSource = courses;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сортировки курсов: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSortDesc_Click(object sender, RoutedEventArgs e) 
        {
            try
            {
                dgCourses.ItemsSource = null;
                var courses = _courseService.GetCoursesSortedByPrice(false);
                for (int i = 0; i < courses.Count; i++)
                {
                    courses[i].DisplayPrice = courses[i].Price.ToString("F2");
                }
                dgCourses.ItemsSource = courses;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сортировки курсов: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnManageModules_Click(object sender, RoutedEventArgs e) 
        {
            if (dgCourses.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите курс для управления модулями.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var selectedCourse = (Course)dgCourses.SelectedItem;
            try
            {
                ModuleManagementWindow moduleWindow = new ModuleManagementWindow(selectedCourse.CourseID, selectedCourse.Title, _moduleService);
                moduleWindow.Owner = this;
                moduleWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии окна управления модулями: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgCourses_SelectionChanged(object sender, SelectionChangedEventArgs e) 
        {
        }

        private void LoadThemes() // Загрузка тем
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString))
                {
                    conn.Open();
                    string query = "SELECT DISTINCT Theme FROM course ORDER BY Theme";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string theme = reader["Theme"].ToString();
                            ComboBoxItem item = new ComboBoxItem { Content = theme };
                            cmbThemeFilter.Items.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки тем: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadAvailableCourses() 
        {
            if (_userType != "registered_user") return;

            try
            {
                var allCourses = _courseService.GetAllCourses();
                var enrolledCourses = GetEnrolledCourses();
                var availableCourses = new List<Course>();
                for (int i = 0; i < allCourses.Count; i++)
                {
                    bool isEnrolled = false;
                    for (int j = 0; j < enrolledCourses.Count; j++)
                    {
                        if (enrolledCourses[j].CourseID == allCourses[i].CourseID)
                        {
                            isEnrolled = true;
                            break;
                        }
                    }
                    if (!isEnrolled)
                        availableCourses.Add(allCourses[i]);
                }
                string selectedTheme = (cmbThemeFilter.SelectedItem as ComboBoxItem)?.Content?.ToString();
                if (selectedTheme != "Все темы")
                {
                    var filteredCourses = new List<Course>();
                    for (int i = 0; i < availableCourses.Count; i++)
                    {
                        if (availableCourses[i].Theme == selectedTheme)
                            filteredCourses.Add(availableCourses[i]);
                    }
                    availableCourses = filteredCourses;
                }
                dgAvailableCourses.ItemsSource = availableCourses;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки доступных курсов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AvailableSearch_TextChanged(object sender, TextChangedEventArgs e) // Поиск доступных курсов
        {
            try
            {
                string searchTerm = txtAvailableSearch.Text;
                var searchResults = string.IsNullOrWhiteSpace(searchTerm) ? _courseService.GetAllCourses() : _courseService.SearchCourses(searchTerm);
                var enrolledCourses = GetEnrolledCourses();
                var filteredResults = new List<Course>();
                for (int i = 0; i < searchResults.Count; i++)
                {
                    bool isEnrolled = false;
                    for (int j = 0; j < enrolledCourses.Count; j++)
                    {
                        if (enrolledCourses[j].CourseID == searchResults[i].CourseID)
                        {
                            isEnrolled = true;
                            break;
                        }
                    }
                    if (!isEnrolled)
                        filteredResults.Add(searchResults[i]);
                }
                string selectedTheme = (cmbThemeFilter.SelectedItem as ComboBoxItem)?.Content?.ToString();
                if (selectedTheme != "Все темы")
                {
                    var themeFiltered = new List<Course>();
                    for (int i = 0; i < filteredResults.Count; i++)
                    {
                        if (filteredResults[i].Theme == selectedTheme)
                            themeFiltered.Add(filteredResults[i]);
                    }
                    filteredResults = themeFiltered;
                }
                dgAvailableCourses.ItemsSource = filteredResults;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска курсов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ThemeFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) 
        {
            LoadAvailableCourses();
        }

        private List<Enrollment> GetEnrolledCourses() 
        {
            List<Enrollment> enrollments = new List<Enrollment>();
            using (MySqlConnection conn = new MySqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"
                        SELECT c.*, e.EnrollmentID, e.UserID, e.CourseID, e.EnrollmentDate, e.CompletionStatus
                        FROM course c
                        JOIN enrollment e ON c.CourseID = e.CourseID
                        WHERE e.UserID = @userId";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@userId", _userId);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            enrollments.Add(new Enrollment
                            {
                                EnrollmentID = reader.GetInt32("EnrollmentID"),
                                UserID = reader.GetInt32("UserID"),
                                CourseID = reader.GetInt32("CourseID"),
                                EnrollmentDate = reader.GetDateTime("EnrollmentDate"),
                                CompletionStatus = reader.GetString("CompletionStatus"),
                                CourseTitle = reader.GetString("Title")
                            });
                        }
                    }
                    return enrollments;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при получении записей: {ex.Message}");
                    throw new Exception("Ошибка доступа к базе данных при получении записей.", ex);
                }
            }
        }

        private void LoadMyCourses() 
        {
            if (_userType != "registered_user") return;

            try
            {
                dgMyCourses.ItemsSource = GetEnrolledCourses();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки моих курсов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEnroll_Click(object sender, RoutedEventArgs e) 
        {
            if (dgAvailableCourses.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите курс для записи.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedCourse = (Course)dgAvailableCourses.SelectedItem;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString))
                {
                    conn.Open();
                    string query = @"
                        INSERT INTO enrollment (UserID, CourseID, EnrollmentDate, CompletionStatus)
                        VALUES (@userId, @courseId, @enrollmentDate, 'not_completed')";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@userId", _userId);
                    cmd.Parameters.AddWithValue("@courseId", selectedCourse.CourseID);
                    cmd.Parameters.AddWithValue("@enrollmentDate", DateTime.Now.Date);
                    cmd.ExecuteNonQuery();

                    var modules = _moduleService.GetModulesByCourseId(selectedCourse.CourseID);
                    for (int i = 0; i < modules.Count; i++)
                    {
                        string statusQuery = @"
                            INSERT INTO moduleStatus (EnrollmentID, ModuleID, Status, CompletionDate)
                            VALUES ((SELECT EnrollmentID FROM enrollment WHERE UserID = @userId AND CourseID = @courseId), @moduleId, 'NotStarted', NULL)";
                        MySqlCommand statusCmd = new MySqlCommand(statusQuery, conn);
                        statusCmd.Parameters.AddWithValue("@userId", _userId);
                        statusCmd.Parameters.AddWithValue("@courseId", selectedCourse.CourseID);
                        statusCmd.Parameters.AddWithValue("@moduleId", modules[i].ModuleID);
                        statusCmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Вы успешно записались на курс!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadAvailableCourses();
                LoadMyCourses();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при записи на курс: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnManageProgress_Click(object sender, RoutedEventArgs e) 
        {
            if (dgMyCourses.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите курс для управления прогрессом.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedEnrollment = (Enrollment)dgMyCourses.SelectedItem;
            _selectedCourseIdForProgress = selectedEnrollment.CourseID;
            string courseTitle = selectedEnrollment.CourseTitle;
            _enrollmentIdForProgress = selectedEnrollment.EnrollmentID;

            lblCourseTitle.Text = $"Прогресс курса: {courseTitle}";
            LoadProgress();
            progressTab.Visibility = Visibility.Visible;
            tabControl.SelectedIndex = tabControl.Items.Count - 1;
        }

        private void LoadProgress() 
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT m.Title, ms.Status, ms.CompletionDate
                        FROM module m
                        JOIN moduleStatus ms ON m.ModuleID = ms.ModuleID
                        WHERE ms.EnrollmentID = @enrollmentId
                        ORDER BY m.ModuleID";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@enrollmentId", _enrollmentIdForProgress);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        List<ModuleStatus> moduleStatuses = new List<ModuleStatus>();
                        while (reader.Read())
                        {
                            moduleStatuses.Add(new ModuleStatus
                            {
                                Status = reader.GetString("Status"),
                                CompletionDate = reader.IsDBNull(reader.GetOrdinal("CompletionDate")) ? (DateTime?)null : reader.GetDateTime("CompletionDate"),
                                ModuleTitle = reader.GetString("Title")
                            });
                        }
                        dgProgress.ItemsSource = moduleStatuses;

                        int totalModules = moduleStatuses.Count;
                        int completedModules = 0;
                        for (int i = 0; i < moduleStatuses.Count; i++)
                        {
                            if (moduleStatuses[i].Status == "Completed")
                                completedModules++;
                        }
                        double percentage = totalModules > 0 ? (completedModules * 100.0 / totalModules) : 0;

                        lblStatsTotal.Text = $"Всего модулей: {totalModules}";
                        lblStatsCompleted.Text = $"Пройдено: {completedModules}";
                        lblStatsPercentage.Text = $"Завершено: {percentage:F1}%";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки прогресса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnMarkCompleted_Click(object sender, RoutedEventArgs e) 
        {
            UpdateModuleStatus("Completed", DateTime.Now.Date);
        }

        private void BtnMarkInProgress_Click(object sender, RoutedEventArgs e) 
        {
            UpdateModuleStatus("InProgress", null);
        }

        private void UpdateModuleStatus(string status, DateTime? completionDate) 
        {
            if (dgProgress.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите модуль для обновления статуса.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedModule = (ModuleStatus)dgProgress.SelectedItem;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString))
                {
                    conn.Open();
                    string query = @"
                        UPDATE moduleStatus ms
                        JOIN module m ON ms.ModuleID = m.ModuleID
                        SET ms.Status = @status, ms.CompletionDate = @completionDate
                        WHERE ms.EnrollmentID = @enrollmentId AND m.Title = @moduleTitle";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@status", status);
                    cmd.Parameters.AddWithValue("@completionDate", completionDate.HasValue ? (object)completionDate.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@enrollmentId", _enrollmentIdForProgress);
                    cmd.Parameters.AddWithValue("@moduleTitle", selectedModule.ModuleTitle);
                    cmd.ExecuteNonQuery();

                    string checkQuery = @"
                        SELECT COUNT(*) 
                        FROM moduleStatus ms
                        JOIN module m ON ms.ModuleID = m.ModuleID
                        WHERE ms.EnrollmentID = @enrollmentId AND ms.Status != 'Completed'";
                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn);
                    checkCmd.Parameters.AddWithValue("@enrollmentId", _enrollmentIdForProgress);
                    int incompleteCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                    string updateEnrollmentQuery = @"
                        UPDATE enrollment
                        SET CompletionStatus = @status
                        WHERE EnrollmentID = @enrollmentId";
                    MySqlCommand updateCmd = new MySqlCommand(updateEnrollmentQuery, conn);
                    updateCmd.Parameters.AddWithValue("@status", incompleteCount == 0 ? "completed" : "not_completed");
                    updateCmd.Parameters.AddWithValue("@enrollmentId", _enrollmentIdForProgress);
                    updateCmd.ExecuteNonQuery();
                }
                MessageBox.Show($"Статус модуля '{selectedModule.ModuleTitle}' обновлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadProgress();
                LoadMyCourses();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении статуса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBackToMyCourses_Click(object sender, RoutedEventArgs e) 
        {
            progressTab.Visibility = Visibility.Collapsed;
            tabControl.SelectedIndex = tabControl.Items.IndexOf(myCoursesTab);
        }

        private void PriceTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e) 
        {
            e.Handled = !decimal.TryParse(e.Text, out _);
        }
    }
}