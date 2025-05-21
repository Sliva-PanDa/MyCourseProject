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
        void AddCourse(Course course);
        void UpdateCourse(Course course);
        Course GetCourseById(int courseId); 
    }
}
