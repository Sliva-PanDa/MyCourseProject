using Cursach.DAL;
using Cursach.Models;
using System;
using System.Collections.Generic;

namespace Cursach.BLL
{
    public class ModuleService : IModuleService
    {
        private readonly IModuleDAL _moduleDAL;

        public ModuleService(IModuleDAL moduleDAL) 
        {
            _moduleDAL = moduleDAL;
            if (_moduleDAL == null) Console.WriteLine("DAL пустой");
        }

        public List<Module> GetModulesByCourseId(int courseId) 
        {
            return _moduleDAL.GetModulesByCourseId(courseId);
        }

        public void AddModule(int courseId, string title, string content) 
        {
            if (string.IsNullOrWhiteSpace(title))
                return;
            _moduleDAL.AddModule(courseId, title, content);
        }

        public void DeleteModule(int moduleId) 
        {
            if (moduleId <= 0)
                return;
            _moduleDAL.DeleteModule(moduleId);
        }
    }
}