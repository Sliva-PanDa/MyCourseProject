using Cursach.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace Cursach.DAL
{
    public class CourseDAL : ICourseDAL
    {
        private string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        public List<Course> GetAllCourses() 
        {
            List<Course> courses = new List<Course>();
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(@"
                    SELECT c.CourseID, c.Title, c.Description, c.Price, c.Theme, u.UserName as InstructorName, c.InstructorID
                    FROM course c LEFT JOIN user u ON c.InstructorID = u.UserID", conn);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        courses.Add(new Course
                        {
                            CourseID = reader.GetInt32("CourseID"),
                            Title = reader.GetString("Title"),
                            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString("Description"),
                            Price = reader.GetDecimal("Price"),
                            Theme = reader.GetString("Theme"),
                            InstructorID = reader.IsDBNull(reader.GetOrdinal("InstructorID")) ? (int?)null : reader.GetInt32("InstructorID"),
                            InstructorName = reader.IsDBNull(reader.GetOrdinal("InstructorName")) ? null : reader.GetString("InstructorName")
                        });
                    }
                }
            }
            return courses;
        }

        public List<Course> SearchCourses(string searchTerm) 
        {
            List<Course> courses = new List<Course>();
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(@"
                    SELECT c.CourseID, c.Title, c.Description, c.Price, c.Theme, u.UserName as InstructorName, c.InstructorID
                    FROM course c LEFT JOIN user u ON c.InstructorID = u.UserID
                    WHERE c.Title LIKE @search OR c.Theme LIKE @search", conn);
                cmd.Parameters.AddWithValue("@search", "%" + searchTerm + "%");
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        courses.Add(new Course
                        {
                            CourseID = reader.GetInt32("CourseID"),
                            Title = reader.GetString("Title"),
                            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString("Description"),
                            Price = reader.GetDecimal("Price"),
                            Theme = reader.GetString("Theme"),
                            InstructorID = reader.IsDBNull(reader.GetOrdinal("InstructorID")) ? (int?)null : reader.GetInt32("InstructorID"),
                            InstructorName = reader.IsDBNull(reader.GetOrdinal("InstructorName")) ? null : reader.GetString("InstructorName")
                        });
                    }
                }
            }
            return courses;
        }

        public void AddCourse(Course course) 
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(@"
                    INSERT INTO course (Title, Description, Price, InstructorID, Theme)
                    VALUES (@title, @description, @price, @instructorId, @theme)", conn);
                cmd.Parameters.AddWithValue("@title", course.Title);
                cmd.Parameters.AddWithValue("@description", course.Description ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@price", course.Price);
                cmd.Parameters.AddWithValue("@instructorId", course.InstructorID.HasValue ? course.InstructorID.Value : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@theme", course.Theme);
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateCourse(Course course) 
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(@"
                    UPDATE course SET Title = @title, Description = @description, Price = @price,
                    InstructorID = @instructorId, Theme = @theme WHERE CourseID = @id", conn);
                cmd.Parameters.AddWithValue("@title", course.Title);
                cmd.Parameters.AddWithValue("@description", course.Description ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@price", course.Price);
                cmd.Parameters.AddWithValue("@instructorId", course.InstructorID.HasValue ? course.InstructorID.Value : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@theme", course.Theme);
                cmd.Parameters.AddWithValue("@id", course.CourseID);
                cmd.ExecuteNonQuery();
            }
        }

        public List<Course> GetCoursesSortedByPrice(bool ascending) 
        {
            List<Course> courses = new List<Course>();
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string sortDirection = ascending ? "ASC" : "DESC";
                MySqlCommand cmd = new MySqlCommand($@"
                    SELECT c.CourseID, c.Title, c.Description, c.Price, c.Theme, u.UserName as InstructorName, c.InstructorID
                    FROM course c LEFT JOIN user u ON c.InstructorID = u.UserID
                    ORDER BY c.Price {sortDirection}", conn);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        courses.Add(new Course
                        {
                            CourseID = reader.GetInt32("CourseID"),
                            Title = reader.GetString("Title"),
                            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString("Description"),
                            Price = reader.GetDecimal("Price"),
                            Theme = reader.GetString("Theme"),
                            InstructorID = reader.IsDBNull(reader.GetOrdinal("InstructorID")) ? (int?)null : reader.GetInt32("InstructorID"),
                            InstructorName = reader.IsDBNull(reader.GetOrdinal("InstructorName")) ? null : reader.GetString("InstructorName")
                        });
                    }
                }
            }
            return courses;
        }
    }
}