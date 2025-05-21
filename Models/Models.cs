using System;

namespace Cursach.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string UserType { get; set; }
    }

    public class Course
    {
        public int CourseID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Theme { get; set; }
        public int? InstructorID { get; set; }
        public string InstructorName { get; set; }
        public string DisplayPrice { get; set; } //           -отображения цены как строки
    }

    public class Module
    {
        public int ModuleID { get; set; }
        public int CourseID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }

    public class Enrollment
    {
        public int EnrollmentID { get; set; }
        public int UserID { get; set; }
        public int CourseID { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public string CompletionStatus { get; set; }
        public string CourseTitle { get; set; }
    }

    public class ModuleStatus
    {
        public int StatusID { get; set; }
        public int EnrollmentID { get; set; }
        public int ModuleID { get; set; }
        public string Status { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string ModuleTitle { get; set; }
    }
}