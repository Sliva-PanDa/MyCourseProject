using Cursach.DAL;
using Cursach.Models;
using System;
using System.Collections.Generic;

namespace Cursach.BLL
{
    public class UserService : IUserService
    {
        private readonly IUserDAL _userDAL;

        public UserService(IUserDAL userDAL)
        {
            _userDAL = userDAL ?? throw new ArgumentNullException(nameof(userDAL));
        }

        public List<User> GetAllUsers()
        {
            return _userDAL.GetAllUsers();
        }

        public bool ValidateUser(string username, string password)
        {
            return _userDAL.ValidateUser(username, password);
        }

        public List<User> GetInstructors()
        {
            return _userDAL.GetInstructors();
        }

        public string GetUserType(string username)
        {
            return _userDAL.GetUserType(username);
        }

        public int GetUserIdByUsername(string username)
        {
            return _userDAL.GetUserIdByUsername(username);
        }
    }
}