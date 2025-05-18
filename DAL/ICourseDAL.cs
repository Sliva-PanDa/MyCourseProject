using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cursach.Models;

namespace Cursach.DAL
{
    public interface ICourseDAL
    {
        List<Course> GetAllCourses();
        List<Course> SearchCourses(string searchTerm);
        void AddCourse(Course course);
        void UpdateCourse(Course course);
        List<Course> GetCoursesSortedByPrice(bool ascending);
    }
}
