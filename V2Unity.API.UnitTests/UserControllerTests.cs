using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using V2Unity.API.Controllers;
using V2Unity.Model;
using V2Unity.API.Persistence;

namespace V2Unity.API.UnitTests
{
    [TestClass]
    public class UserControllerTests
    {
        [TestMethod]
        public void GetUsersTest()
        {
            var existingUser = new User()
            {
                Id = Guid.NewGuid(),
                Name = "Mock User",
                DeviceId = Guid.NewGuid().ToString()
            };

            var mockUserRepo = new Mock<IRepository<User>>();
            mockUserRepo.Setup(m => m.FindAll()).Returns(new List<User>() { existingUser });

            var controller = new UsersController(mockUserRepo.Object);

            var result = controller.GetUsers();

            Assert.IsInstanceOfType<OkObjectResult>(result);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.IsNotNull(okResult.Value);

            var users = okResult.Value as IEnumerable<User>;
            
            Assert.IsNotNull(users);
            Assert.AreEqual(1, users.Count());

            mockUserRepo.Verify();
        }

        [TestMethod]
        public void CreateUserTest()
        {
            var mockUserRepo = new Mock<IRepository<User>>();
            mockUserRepo.Setup(r => r.FindOne(It.IsAny<Expression<Func<User, bool>>>())).Returns((User?)null);

            var controller = new UsersController(mockUserRepo.Object);
            var result = controller.CreateUser(new User() { DeviceId = Guid.NewGuid().ToString(), Name = "Mock User" });
            Assert.IsInstanceOfType<CreatedResult>(result);

            mockUserRepo.Verify();
        }

        [TestMethod]
        public void CreateUser_InvalidUsername_Test()
        {
            var mockUserRepo = new Mock<IRepository<User>>();


            var controller = new UsersController(mockUserRepo.Object);
            var result = controller.CreateUser(new User() { DeviceId = Guid.NewGuid().ToString(), Name = "" });
            Assert.IsInstanceOfType<BadRequestObjectResult>(result);

            result = controller.CreateUser(new User() { DeviceId = Guid.NewGuid().ToString(), Name = "a" });
            Assert.IsInstanceOfType<BadRequestObjectResult>(result);
        }

        [TestMethod]
        public void CreateUser_DuplicateUsername_Test()
        {
            var mockUserRepo = new Mock<IRepository<User>>();

            // When the controller looks for a user, pretend to find one
            mockUserRepo.Setup(m => m.FindOne(It.IsAny<Expression<Func<User, bool>>>())).Returns(new User());



            var controller = new UsersController(mockUserRepo.Object);
            var result = controller.CreateUser(new User() { DeviceId = Guid.NewGuid().ToString(), Name = "Mock User" });

            Assert.IsInstanceOfType<ConflictObjectResult>(result);

            mockUserRepo.Verify();
        }

        [TestMethod]
        public void UpdateUserTest()
        {
            var existingUser = new User()
            {
                Id = Guid.NewGuid(),
                Name = "Mock User",
                DeviceId = Guid.NewGuid().ToString()
            };

            var mockUserRepo = new Mock<IRepository<User>>();
            mockUserRepo.Setup(m => m.FindOne(It.IsAny<Expression<Func<User, bool>>>())).Returns(existingUser);
            mockUserRepo.Setup(m => m.Update(It.Is<User>(user => user.DeviceId == existingUser.DeviceId))).Returns(true);


            var controller = new UsersController(mockUserRepo.Object);
            var result = controller.UpdateUser(existingUser.DeviceId, "Mock User 2");
            Assert.IsInstanceOfType<OkResult>(result);

            mockUserRepo.Verify();
        }

        [TestMethod]
        public void UpdateUser_UserNotFound_Test()
        {
            var deviceId = Guid.NewGuid().ToString();
            var mockUserRepo = new Mock<IRepository<User>>();
            mockUserRepo.Setup(m => m.FindOne(It.IsAny<Expression<Func<User, bool>>>())).Returns((User?)null);


            var controller = new UsersController(mockUserRepo.Object);
            var result = controller.UpdateUser(deviceId, "Mock User 2");
            Assert.IsInstanceOfType<NotFoundObjectResult>(result);

            mockUserRepo.Verify();
        }

        [TestMethod]
        public void LoginUserTest()
        {
            var existingUser = new User()
            {
                Id = Guid.NewGuid(),
                Name = "Mock User",
                DeviceId = Guid.NewGuid().ToString()
            };

            var mockUserRepo = new Mock<IRepository<User>>();
            mockUserRepo.Setup(m => m.FindOne(It.IsAny<Expression<Func<User, bool>>>())).Returns(existingUser);


            var controller = new UsersController(mockUserRepo.Object);
            var result = controller.LoginUser(existingUser.DeviceId);
            Assert.IsInstanceOfType<OkObjectResult>(result);

            mockUserRepo.Verify();
        }

        [TestMethod]
        public void LoginUser_NotFound_Test()
        {
            var mockUserRepo = new Mock<IRepository<User>>();

            var controller = new UsersController(mockUserRepo.Object);
            var result = controller.LoginUser(Guid.NewGuid().ToString());
            Assert.IsInstanceOfType<NotFoundObjectResult>(result);
        }

        [TestMethod]
        public void DeleteUserTest()
        {
            var existingUser = new User()
            {
                Id = Guid.NewGuid(),
                Name = "Mock User",
                DeviceId = Guid.NewGuid().ToString()
            };

            var mockUserRepo = new Mock<IRepository<User>>();
            mockUserRepo.Setup(m => m.FindOne(It.IsAny<Expression<Func<User, bool>>>())).Returns(existingUser);
            mockUserRepo.Setup(m => m.Delete(It.IsAny<Guid>())).Returns(true);


            var controller = new UsersController(mockUserRepo.Object);
            var result = controller.DeleteUser(existingUser.DeviceId);
            Assert.IsInstanceOfType<OkResult>(result);
        }

        [TestMethod]
        public void DeleteUser_NotFound_Test()
        {
            var mockUserRepo = new Mock<IRepository<User>>();


            var controller = new UsersController(mockUserRepo.Object);
            var result = controller.DeleteUser(Guid.NewGuid().ToString());

#if DEBUG
            Assert.IsInstanceOfType<NotFoundObjectResult>(result);
#else
            Assert.IsInstanceOfType<ForbiddenResult>(result);
#endif
        }
    }
}
