using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cursach.Models;
using System.Threading.Tasks;

namespace Cursach.DAL
{
    public interface IModuleDAL
    {
        List<Module> GetModulesByCourseId(int courseId);
        void AddModule(int courseId, string title, string content);
        void DeleteModule(int moduleId);
    }
}
