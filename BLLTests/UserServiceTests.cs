using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Cursach.BLL;
using Cursach.DAL; // Для IUserDAL
using Cursach.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cursach.Tests.BLLTests
{
    [TestClass]
    public class UserServiceTests
    {
        private Mock<IUserDAL> _mockUserDAL;
        private UserService _userService;

        [TestInitialize]
        public void Setup()
        {
            _mockUserDAL = new Mock<IUserDAL>();
            _userService = new UserService(_mockUserDAL.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullDAL_ShouldThrowArgumentNullException()
        {
            // Act
            var service = new UserService(null);
        }

        [TestMethod]
        public void GetAllUsers_ShouldReturnUsersFromDAL()
        {
            // Arrange
            var expectedUsers = new List<User>
            {
                new User { UserID = 1, UserName = "user1" },
                new User { UserID = 2, UserName = "user2" }
            };
            _mockUserDAL.Setup(dal => dal.GetAllUsers()).Returns(expectedUsers);

            // Act
            var result = _userService.GetAllUsers();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedUsers.Count, result.Count);
            CollectionAssert.AreEqual(expectedUsers, result.ToList());
            _mockUserDAL.Verify(dal => dal.GetAllUsers(), Times.Once);
        }

        [TestMethod]
        public void ValidateUser_ValidCredentials_ShouldReturnTrueFromDAL()
        {
            // Arrange
            string username = "testuser";
            string password = "password";
            _mockUserDAL.Setup(dal => dal.ValidateUser(username, password)).Returns(true);

            // Act
            var result = _userService.ValidateUser(username, password);

            // Assert
            Assert.IsTrue(result);
            _mockUserDAL.Verify(dal => dal.ValidateUser(username, password), Times.Once);
        }

        [TestMethod]
        public void ValidateUser_InvalidCredentials_ShouldReturnFalseFromDAL()
        {
            // Arrange
            string username = "testuser";
            string password = "wrongpassword";
            _mockUserDAL.Setup(dal => dal.ValidateUser(username, password)).Returns(false);

            // Act
            var result = _userService.ValidateUser(username, password);

            // Assert
            Assert.IsFalse(result);
            _mockUserDAL.Verify(dal => dal.ValidateUser(username, password), Times.Once);
        }

        [TestMethod]
        public void GetInstructors_ShouldReturnInstructorsFromDAL()
        {
            // Arrange
            var expectedInstructors = new List<User>
            {
                new User { UserID = 3, UserName = "instructor1", UserType = "Инструктор" },
                new User { UserID = 4, UserName = "instructor2", UserType = "Инструктор" }
            };
            _mockUserDAL.Setup(dal => dal.GetInstructors()).Returns(expectedInstructors);

            // Act
            var result = _userService.GetInstructors();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedInstructors.Count, result.Count);
            CollectionAssert.AreEqual(expectedInstructors, result.ToList());
            _mockUserDAL.Verify(dal => dal.GetInstructors(), Times.Once);
        }

        [TestMethod]
        public void GetUserType_ExistingUser_ShouldReturnUserTypeFromDAL()
        {
            // Arrange
            string username = "testuser";
            string expectedType = "Студент";
            _mockUserDAL.Setup(dal => dal.GetUserType(username)).Returns(expectedType);

            // Act
            var result = _userService.GetUserType(username);

            // Assert
            Assert.AreEqual(expectedType, result);
            _mockUserDAL.Verify(dal => dal.GetUserType(username), Times.Once);
        }

        [TestMethod]
        public void GetUserType_NonExistingUser_ShouldReturnNullFromDAL()
        {
            // Arrange
            string username = "nonexistinguser";
            _mockUserDAL.Setup(dal => dal.GetUserType(username)).Returns((string)null);

            // Act
            var result = _userService.GetUserType(username);

            // Assert
            Assert.IsNull(result);
            _mockUserDAL.Verify(dal => dal.GetUserType(username), Times.Once);
        }

        [TestMethod]
        public void GetUserIdByUsername_ExistingUser_ReturnsCorrectId() // Переименовал для ясности
        {
            // Arrange
            string username = "testuser";
            int expectedId = 1; // Допустим, мы ожидаем ID = 1 для этого пользователя
            _mockUserDAL.Setup(dal => dal.GetUserIdByUsername(username)).Returns(expectedId);

            // Act
            var result = _userService.GetUserIdByUsername(username);

            // Assert
            Assert.AreEqual(expectedId, result, "ID пользователя не совпадает."); // Добавил сообщение
            _mockUserDAL.Verify(dal => dal.GetUserIdByUsername(username), Times.Once);
        }

        [TestMethod]
        public void GetUserIdByUsername_WhenUserNotFound_ReturnsMinusOne() // Исправленный тест
        {
            // Arrange
            string username = "nonexistinguser";
            int expectedIdIfNotFound = -1; // UserDAL возвращает -1, если пользователь не найден
            _mockUserDAL.Setup(dal => dal.GetUserIdByUsername(username)).Returns(expectedIdIfNotFound);

            // Act
            var result = _userService.GetUserIdByUsername(username);

            // Assert
            Assert.AreEqual(expectedIdIfNotFound, result, "Ожидалось -1 для несуществующего пользователя.");
            _mockUserDAL.Verify(dal => dal.GetUserIdByUsername(username), Times.Once);
        }
    }
}