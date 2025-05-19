using Cursach.DAL;
using Cursach.Models;
using System.Collections.Generic;

namespace Cursach.BLL
{
    public class CourseService : ICourseService
    {
        private readonly ICourseDAL _courseDAL;

        public CourseService(ICourseDAL courseDAL)
        {
            _courseDAL = courseDAL;
        }

        public List<Course> GetAllCourses()
        {
            return _courseDAL.GetAllCourses();
        }

        public void AddCourse(Course course)
        {
            _courseDAL.AddCourse(course);
        }

        public void UpdateCourse(Course course)
        {
            _courseDAL.UpdateCourse(course);
        }

        public Course GetCourseById(int courseId)
        {
            return _courseDAL.GetCourseById(courseId);
        }
    }
}