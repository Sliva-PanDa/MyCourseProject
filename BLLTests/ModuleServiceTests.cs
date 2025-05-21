using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Cursach.BLL;
using Cursach.DAL; // Для IModuleDAL
using Cursach.Models;
using System.Collections.Generic;
using System.Linq;

namespace Cursach.Tests.BLLTests
{
    [TestClass]
    public class ModuleServiceTests
    {
        private Mock<IModuleDAL> _mockModuleDAL;
        private ModuleService _moduleService;

        [TestInitialize]
        public void Setup()
        {
            _mockModuleDAL = new Mock<IModuleDAL>();
            _moduleService = new ModuleService(_mockModuleDAL.Object);
        }

        [TestMethod]
        public void GetModulesByCourseId_ShouldReturnModulesFromDAL()
        {
            // Arrange
            int courseId = 1;
            var expectedModules = new List<Module>
            {
                new Module { ModuleID = 10, CourseID = courseId, Title = "Модуль 1" },
                new Module { ModuleID = 11, CourseID = courseId, Title = "Модуль 2" }
            };
            _mockModuleDAL.Setup(dal => dal.GetModulesByCourseId(courseId)).Returns(expectedModules);

            // Act
            var result = _moduleService.GetModulesByCourseId(courseId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedModules.Count, result.Count);
            CollectionAssert.AreEqual(expectedModules, result.ToList());
            _mockModuleDAL.Verify(dal => dal.GetModulesByCourseId(courseId), Times.Once);
        }

        [TestMethod]
        public void AddModule_ValidTitle_ShouldCallAddModuleOnDAL()
        {
            // Arrange
            int courseId = 1;
            string title = "Новый модуль";
            string content = "Содержимое модуля";

            // Act
            _moduleService.AddModule(courseId, title, content);

            // Assert
            _mockModuleDAL.Verify(dal => dal.AddModule(courseId, title, content), Times.Once);
        }

        [TestMethod]
        public void AddModule_EmptyTitle_ShouldNotCallAddModuleOnDAL()
        {
            // Arrange
            int courseId = 1;
            string title = "   "; // Пустой или пробельный
            string content = "Содержимое модуля";

            // Act
            _moduleService.AddModule(courseId, title, content);

            // Assert
            _mockModuleDAL.Verify(dal => dal.AddModule(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public void AddModule_NullTitle_ShouldNotCallAddModuleOnDAL()
        {
            // Arrange
            int courseId = 1;
            string title = null;
            string content = "Содержимое модуля";

            // Act
            _moduleService.AddModule(courseId, title, content);

            // Assert
            _mockModuleDAL.Verify(dal => dal.AddModule(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public void DeleteModule_ValidId_ShouldCallDeleteModuleOnDAL()
        {
            // Arrange
            int moduleId = 1;

            // Act
            _moduleService.DeleteModule(moduleId);

            // Assert
            _mockModuleDAL.Verify(dal => dal.DeleteModule(moduleId), Times.Once);
        }

        [TestMethod]
        public void DeleteModule_InvalidId_Zero_ShouldNotCallDeleteModuleOnDAL()
        {
            // Arrange
            int moduleId = 0;

            // Act
            _moduleService.DeleteModule(moduleId);

            // Assert
            _mockModuleDAL.Verify(dal => dal.DeleteModule(It.IsAny<int>()), Times.Never);
        }

        [TestMethod]
        public void DeleteModule_InvalidId_Negative_ShouldNotCallDeleteModuleOnDAL()
        {
            // Arrange
            int moduleId = -5;

            // Act
            _moduleService.DeleteModule(moduleId);

            // Assert
            _mockModuleDAL.Verify(dal => dal.DeleteModule(It.IsAny<int>()), Times.Never);
        }
    }
}