using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cursach.Models;

namespace Cursach.DAL
{
    public interface IUserDAL
    {
        List<User> GetAllUsers();
        bool ValidateUser(string username, string password);
        List<User> GetInstructors();
        string GetUserType(string username);
        int GetUserIdByUsername(string username);
    }
}
