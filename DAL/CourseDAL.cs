using Cursach.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace Cursach.DAL
{
    public class CourseDAL : ICourseDAL
    {
        private readonly string _connectionString;

        public CourseDAL()
        {
            _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        }

        public List<Course> GetAllCourses()
        {
            List<Course> courses = new List<Course>();
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                MySqlCommand checkCmd = new MySqlCommand("SELECT COUNT(*) FROM course WHERE CourseID = 2", conn);
                int courseCount = Convert.ToInt32(checkCmd.ExecuteScalar());
                Console.WriteLine($"Проверка: Количество курсов с CourseID=2: {courseCount}");

                MySqlCommand cmd = new MySqlCommand("SELECT c.*, u.UserName FROM course c LEFT JOIN user u ON c.InstructorID = u.UserID", conn);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var course = new Course
                        {
                            CourseID = reader.GetInt32("CourseID"),
                            Title = reader.IsDBNull(reader.GetOrdinal("Title")) ? "Без названия" : reader.GetString("Title"),
                            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? "" : reader.GetString("Description"),
                            Price = reader.GetDecimal("Price"),
                            InstructorID = reader.IsDBNull(reader.GetOrdinal("InstructorID")) ? 0 : reader.GetInt32("InstructorID"),
                            InstructorName = reader.IsDBNull(reader.GetOrdinal("UserName")) ? "Неизвестный" : reader.GetString("UserName"),
                            Theme = reader.IsDBNull(reader.GetOrdinal("Theme")) ? "" : reader.GetString("Theme")
                        };
                        courses.Add(course);
                        Console.WriteLine($"Курс загружен: ID={course.CourseID}, Title={course.Title}, InstructorID={course.InstructorID}, InstructorName={course.InstructorName}");
                    }
                }
            }
            Console.WriteLine($"Всего загружено курсов: {courses.Count}");
            return courses;
        }

        public Course GetCourseById(int courseId)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT c.*, u.UserName FROM course c JOIN user u ON c.InstructorID = u.UserID WHERE c.CourseID = @courseId", conn);
                cmd.Parameters.AddWithValue("@courseId", courseId);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Course
                        {
                            CourseID = reader.GetInt32("CourseID"),
                            Title = reader.GetString("Title"),
                            Description = reader.GetString("Description"),
                            Price = reader.GetDecimal("Price"),
                            InstructorID = reader.GetInt32("InstructorID"),
                            InstructorName = reader.GetString("UserName"),
                            Theme = reader.GetString("Theme")
                        };
                    }
                }
            }
            return null;
        }

        public void AddCourse(Course course)
        {
            Console.WriteLine($"Добавление курса: Title={course.Title}, InstructorID={course.InstructorID}");
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(
                    "INSERT INTO course (Title, Description, Price, InstructorID, Theme) VALUES (@Title, @Description, @Price, @InstructorID, @Theme)",
                    conn);
                cmd.Parameters.AddWithValue("@Title", course.Title);
                cmd.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(course.Description) ? (object)DBNull.Value : course.Description);
                cmd.Parameters.AddWithValue("@Price", course.Price);
                cmd.Parameters.AddWithValue("@InstructorID", course.InstructorID == 0 ? (object)DBNull.Value : course.InstructorID);
                cmd.Parameters.AddWithValue("@Theme", course.Theme);
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateCourse(Course course)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("UPDATE course SET Title = @title, Description = @description, Price = @price, InstructorID = @instructorId, Theme = @theme WHERE CourseID = @courseId", conn);
                cmd.Parameters.AddWithValue("@title", course.Title);
                cmd.Parameters.AddWithValue("@description", course.Description);
                cmd.Parameters.AddWithValue("@price", course.Price);
                cmd.Parameters.AddWithValue("@instructorId", course.InstructorID);
                cmd.Parameters.AddWithValue("@theme", course.Theme);
                cmd.Parameters.AddWithValue("@courseId", course.CourseID);
                cmd.ExecuteNonQuery();
            }
        }

    }
}