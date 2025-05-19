using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cursach.Models;

namespace Cursach.BLL
{
    public interface IModuleService
    {
        List<Module> GetModulesByCourseId(int courseId);
        void AddModule(int courseId, string title, string content);
        void DeleteModule(int moduleId);
    }
}
