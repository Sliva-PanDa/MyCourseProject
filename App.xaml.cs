using Cursach.BLL;
using Cursach.DAL;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace Cursach
{
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            services.AddSingleton<ICourseDAL, CourseDAL>();
            services.AddSingleton<IModuleDAL, ModuleDAL>();
            services.AddSingleton<IUserDAL, UserDAL>();
            services.AddSingleton<ICourseService, CourseService>();
            services.AddSingleton<IModuleService, ModuleService>();
            services.AddSingleton<IUserService, UserService>();
            services.AddTransient<MainWindow>(); 
            _serviceProvider = services.BuildServiceProvider();

            var mainWindow = _serviceProvider.GetService<MainWindow>(); 
            mainWindow.Show();
        }

        public IServiceProvider ServiceProvider => _serviceProvider;
    }
}