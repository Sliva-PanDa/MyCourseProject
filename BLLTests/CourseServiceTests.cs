using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Cursach.BLL;
using Cursach.DAL; // Для ICourseDAL
using Cursach.Models;
using System.Collections.Generic;
using System.Linq;

namespace Cursach.Tests.BLLTests
{
    [TestClass]
    public class CourseServiceTests
    {
        private Mock<ICourseDAL> _mockCourseDAL;
        private CourseService _courseService;

        [TestInitialize]
        public void Setup()
        {
            _mockCourseDAL = new Mock<ICourseDAL>();
            _courseService = new CourseService(_mockCourseDAL.Object);
        }

        [TestMethod]
        public void GetAllCourses_ShouldReturnCoursesFromDAL()
        {
            // Arrange
            var expectedCourses = new List<Course>
            {
                new Course { CourseID = 1, Title = "Курс 1" },
                new Course { CourseID = 2, Title = "Курс 2" }
            };
            _mockCourseDAL.Setup(dal => dal.GetAllCourses()).Returns(expectedCourses);

            // Act
            var result = _courseService.GetAllCourses();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedCourses.Count, result.Count);
            CollectionAssert.AreEqual(expectedCourses, result.ToList()); // Проверяем содержимое списков
            _mockCourseDAL.Verify(dal => dal.GetAllCourses(), Times.Once); // Убедимся, что метод DAL был вызван
        }

        [TestMethod]
        public void GetCourseById_ExistingId_ShouldReturnCourseFromDAL()
        {
            // Arrange
            int courseId = 1;
            var expectedCourse = new Course { CourseID = courseId, Title = "Тестовый курс" };
            _mockCourseDAL.Setup(dal => dal.GetCourseById(courseId)).Returns(expectedCourse);

            // Act
            var result = _courseService.GetCourseById(courseId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedCourse.CourseID, result.CourseID);
            Assert.AreEqual(expectedCourse.Title, result.Title);
            _mockCourseDAL.Verify(dal => dal.GetCourseById(courseId), Times.Once);
        }

        [TestMethod]
        public void GetCourseById_NonExistingId_ShouldReturnNullFromDAL()
        {
            // Arrange
            int courseId = 99;
            _mockCourseDAL.Setup(dal => dal.GetCourseById(courseId)).Returns((Course)null);

            // Act
            var result = _courseService.GetCourseById(courseId);

            // Assert
            Assert.IsNull(result);
            _mockCourseDAL.Verify(dal => dal.GetCourseById(courseId), Times.Once);
        }

        [TestMethod]
        public void AddCourse_ShouldCallAddCourseOnDAL()
        {
            // Arrange
            var courseToAdd = new Course { Title = "Новый курс", Price = 100m };
            // Нам не нужно настраивать Returns, так как метод AddCourse в DAL возвращает void

            // Act
            _courseService.AddCourse(courseToAdd);

            // Assert
            _mockCourseDAL.Verify(dal => dal.AddCourse(courseToAdd), Times.Once); // Проверяем, что метод был вызван с этим объектом
            _mockCourseDAL.Verify(dal => dal.AddCourse(It.Is<Course>(c =>
                c.Title == courseToAdd.Title &&
                c.Price == courseToAdd.Price
            )), Times.Once); // Более детальная проверка переданного объекта
        }

        [TestMethod]
        public void UpdateCourse_ShouldCallUpdateCourseOnDAL()
        {
            // Arrange
            var courseToUpdate = new Course { CourseID = 1, Title = "Обновленный курс", Price = 150m };

            // Act
            _courseService.UpdateCourse(courseToUpdate);

            // Assert
            _mockCourseDAL.Verify(dal => dal.UpdateCourse(courseToUpdate), Times.Once);
            _mockCourseDAL.Verify(dal => dal.UpdateCourse(It.Is<Course>(c =>
                c.CourseID == courseToUpdate.CourseID &&
                c.Title == courseToUpdate.Title &&
                c.Price == courseToUpdate.Price
            )), Times.Once);
        }
    }
}