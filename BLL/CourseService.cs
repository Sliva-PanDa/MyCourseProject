using Cursach.DAL;
using Cursach.Models;
using System;
using System.Collections.Generic;

namespace Cursach.BLL
{
    public class CourseService : ICourseService
    {
        private readonly ICourseDAL _courseDAL;

        public CourseService(ICourseDAL courseDAL) 
        {
            _courseDAL = courseDAL;
            if (_courseDAL == null) Console.WriteLine("DAL пустой");
        }

        public List<Course> GetAllCourses() 
        {
            return _courseDAL.GetAllCourses();
        }

        public List<Course> SearchCourses(string searchTerm) 
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAllCourses();
            return _courseDAL.SearchCourses(searchTerm);
        }

        public void AddCourse(Course course)    
        {
            if (string.IsNullOrWhiteSpace(course.Title))
                return;
            if (course.Price < 0)
                course.Price = 0;
            if (string.IsNullOrWhiteSpace(course.Theme))
                course.Theme = "Без темы";
            _courseDAL.AddCourse(course);
        }

        public void UpdateCourse(Course course) 
        {
            if (course.CourseID <= 0 || string.IsNullOrWhiteSpace(course.Title))
                return;
            if (course.Price < 0)
                course.Price = 0;
            _courseDAL.UpdateCourse(course);
        }

        public List<Course> GetCoursesSortedByPrice(bool ascending) 
        {
            return _courseDAL.GetCoursesSortedByPrice(ascending);
        }
    }
}