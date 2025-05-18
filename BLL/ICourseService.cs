using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Cursach.Models;

namespace Cursach.BLL
{
    public interface ICourseService
    {
        List<Course> GetAllCourses();
        List<Course> SearchCourses(string searchTerm);
        void AddCourse(Course course);
        void UpdateCourse(Course course);
        List<Course> GetCoursesSortedByPrice(bool ascending);
    }
}
