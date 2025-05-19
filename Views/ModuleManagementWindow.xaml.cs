using Cursach.BLL;
using Cursach.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Cursach
{
    public partial class ModuleManagementWindow : Window
    {
        private readonly int _courseId;
        private readonly string _courseTitle;
        private readonly IModuleService _moduleService;

        public ModuleManagementWindow(int courseId, string courseTitle, IModuleService moduleService)
        {
            InitializeComponent();
            _courseId = courseId;
            _courseTitle = courseTitle;
            _moduleService = moduleService;
            this.Title = $"Управление модулями - {_courseTitle}";
            lblCourseTitle.Text = $"Модули курса: {_courseTitle}";
            LoadModules();
        }

        private void LoadModules()
        {
            try
            {
                dgModules.ItemsSource = _moduleService.GetModulesByCourseId(_courseId);
            }
            catch (Exception ex)
            {
                string errorMessage = "Ошибка загрузки модулей: " + ex.Message;
                if (ex.InnerException != null)
                    errorMessage += "\n\nДетали: " + ex.InnerException.Message;
                MessageBox.Show(errorMessage, "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAddModule_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtModuleTitle.Text))
            {
                MessageBox.Show("Пожалуйста, введите название модуля.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtModuleTitle.Focus();
                return;
            }

            try
            {
                _moduleService.AddModule(_courseId, txtModuleTitle.Text, txtModuleContent.Text);
                txtModuleTitle.Clear();
                txtModuleContent.Clear();
                LoadModules();
                MessageBox.Show("Модуль успешно добавлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                string errorMessage = "Ошибка при добавлении модуля: " + ex.Message;
                if (ex.InnerException != null)
                    errorMessage += "\n\nДетали: " + ex.InnerException.Message;
                MessageBox.Show(errorMessage, "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDeleteModule_Click(object sender, RoutedEventArgs e)
        {
            if (dgModules.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите модуль для удаления.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedModule = (Module)dgModules.SelectedItem;
            MessageBoxResult result = MessageBox.Show($"Вы уверены, что хотите удалить модуль '{selectedModule.Title}' (ID: {selectedModule.ModuleID})?",
                                                     "Подтверждение удаления",
                                                     MessageBoxButton.YesNo,
                                                     MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                _moduleService.DeleteModule(selectedModule.ModuleID);
                LoadModules();
                MessageBox.Show("Модуль успешно удален.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                string errorMessage = "Ошибка при удалении модуля: " + ex.Message;
                if (ex.InnerException is MySql.Data.MySqlClient.MySqlException mysqlEx && mysqlEx.Number == 1451)
                    errorMessage += "\n\nВозможно, этот модуль используется в записях о прохождении курсов и не может быть удален.";
                else if (ex.InnerException != null)
                    errorMessage += "\n\nДетали: " + ex.InnerException.Message;
                MessageBox.Show(errorMessage, "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}