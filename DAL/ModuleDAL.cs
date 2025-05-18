using Cursach.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace Cursach.DAL
{
    public class ModuleDAL : IModuleDAL
    {
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        public List<Module> GetModulesByCourseId(int courseId) 
        {
            List<Module> modules = new List<Module>();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT ModuleID, Title, Content FROM module WHERE CourseID = @CourseID ORDER BY ModuleID", conn);
                cmd.Parameters.AddWithValue("@CourseID", courseId);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        modules.Add(new Module
                        {
                            ModuleID = reader.GetInt32("ModuleID"),
                            CourseID = courseId,
                            Title = reader.GetString("Title"),
                            Content = reader.IsDBNull(reader.GetOrdinal("Content")) ? null : reader.GetString("Content")
                        });
                    }
                }
            }
            return modules;
        }

        public void AddModule(int courseId, string title, string content) 
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("INSERT INTO module (CourseID, Title, Content) VALUES (@CourseID, @Title, @Content)", conn);
                cmd.Parameters.AddWithValue("@CourseID", courseId);
                cmd.Parameters.AddWithValue("@Title", title);
                cmd.Parameters.AddWithValue("@Content", content ?? (object)DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteModule(int moduleId) 
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("DELETE FROM module WHERE ModuleID = @ModuleID", conn);
                cmd.Parameters.AddWithValue("@ModuleID", moduleId);
                cmd.ExecuteNonQuery();
            }
        }
    }
}