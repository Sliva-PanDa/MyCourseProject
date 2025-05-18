using Cursach.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace Cursach.DAL
{
    public class UserDAL : IUserDAL
    {
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        public List<User> GetAllUsers()
        {
            List<User> users = new List<User>();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT * FROM user";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new User
                            {
                                UserID = reader.GetInt32("UserID"),
                                UserName = reader.GetString("UserName"),
                                Password = reader.GetString("Password"),
                                UserType = reader.GetString("UserType")
                            });
                        }
                    }
                    return users;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка при получении пользователей: " + ex.Message);
                    throw new Exception("Ошибка доступа к базе данных при получении пользователей.", ex);
                }
            }
        }

        public bool ValidateUser(string username, string password)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM user WHERE UserName = @username AND Password = @password";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка при проверке пользователя: " + ex.Message);
                    throw new Exception("Ошибка доступа к базе данных при проверке пользователя.", ex);
                }
            }
        }

        public List<User> GetInstructors()
        {
            List<User> instructors = new List<User>();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT UserID, UserName FROM user ORDER BY UserName";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            instructors.Add(new User
                            {
                                UserID = reader.GetInt32("UserID"),
                                UserName = reader.GetString("UserName")
                            });
                        }
                    }
                    return instructors;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка при получении инструкторов: " + ex.Message);
                    throw new Exception("Ошибка доступа к базе данных при получении инструкторов.", ex);
                }
            }
        }

        public string GetUserType(string username)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT UserType FROM user WHERE UserName = @username";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@username", username);
                    object result = cmd.ExecuteScalar();
                    return result != null ? result.ToString() : null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка при получении типа пользователя: " + ex.Message);
                    throw new Exception("Ошибка доступа к базе данных при получении типа пользователя.", ex);
                }
            }
        }

        public int GetUserIdByUsername(string username)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT UserID FROM user WHERE UserName = @username";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@username", username);
                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : -1;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка при получении ID пользователя: " + ex.Message);
                    throw new Exception("Ошибка доступа к базе данных при получении ID пользователя.", ex);
                }
            }
        }
    }
}