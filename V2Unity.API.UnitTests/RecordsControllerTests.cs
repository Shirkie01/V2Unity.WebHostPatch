using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using V2Unity.API.Controllers;
using V2Unity.API.Persistence;
using V2Unity.Model;

namespace V2Unity.API.UnitTests
{
    [TestClass]
    public class RecordsControllerTests
    {
        [TestMethod]
        public void GetRecordsForStageTest()
        {
            var existingUser = new User()
            {
                Id = Guid.NewGuid(),
                Name = "Mock User",
                DeviceId = Guid.NewGuid().ToString()
            };

            var existingRecord = new Record()
            {
                Id = Guid.NewGuid(),
                Difficulty = Difficulty.Easy,
                DeviceId = existingUser.DeviceId,
                Name = existingUser.Name,
                TotalEnemiesDestroyed = 7
            };

            var mockUserRepo = new Mock<IRepository<User>>();

            var mockRecordRepo = new Mock<IRepository<Record>>();
            mockRecordRepo.Setup(m => m.FindAll()).Returns(new List<Record>() { existingRecord });

            var controller = new RecordsController(mockUserRepo.Object, mockRecordRepo.Object);

            var result = controller.GetRecordsForStage();
            Assert.IsInstanceOfType<OkObjectResult>(result);

            mockRecordRepo.Verify();
        }

        [TestMethod]
        public void GetRecordsForStage_Empty_Test()
        {
            var mockUserRepo = new Mock<IRepository<User>>();
            var mockRecordRepo = new Mock<IRepository<Record>>();
            var controller = new RecordsController(mockUserRepo.Object, mockRecordRepo.Object);
            var result = controller.GetRecordsForStage();
            Assert.IsInstanceOfType<OkObjectResult>(result);
        }

        [TestMethod]
        public void GetRecordsForStage_WithStage_Test()
        {

        }

        [TestMethod]
        public void GetPersonalRecordsTest()
        {
            var existingUser = new User()
            {
                Id = Guid.NewGuid(),
                Name = "Mock User",
                DeviceId = Guid.NewGuid().ToString()
            };

            var mockUserRepo = new Mock<IRepository<User>>();
            mockUserRepo.Setup(m => m.FindOne(It.IsAny<Expression<Func<User, bool>>>())).Returns(existingUser);

            var mockRecordRepo = new Mock<IRepository<Record>>();
            mockRecordRepo.Setup(m => m.FindAll()).Returns(new List<Record>()
            {
                new Record()
                {
                    Id = Guid.NewGuid(),
                    DeviceId = existingUser.DeviceId,
                    Stage = "STAGE",
                    Difficulty = Difficulty.Easy,
                    TotalEnemiesDestroyed = 3,
                }
            });

            var controller = new RecordsController(mockUserRepo.Object, mockRecordRepo.Object);
            var result = controller.GetPersonalRecords(existingUser.DeviceId, "STAGE");
            Assert.IsInstanceOfType<OkObjectResult>(result);

            var okObjectResult = result as OkObjectResult;
            Assert.IsNotNull(okObjectResult);

            var list = okObjectResult.Value as List<Record>;
            Assert.IsNotNull(list);
            Assert.AreEqual(1, list.Count);

            mockUserRepo.Verify();
            mockRecordRepo.Verify();
        }

        [TestMethod]
        public void NewRecordTest()
        {
            var existingUser = new User()
            {
                Id = Guid.NewGuid(),
                Name = "Mock User",
                DeviceId = Guid.NewGuid().ToString()
            };

            var newRecord = new Record()
            {
                Id = Guid.NewGuid(),
                DeviceId = existingUser.DeviceId,
                Name = existingUser.Name,
                Stage = "STAGE",
                Difficulty = Difficulty.Easy,
                TotalEnemiesDestroyed = 3,
            };

            var mockUserRepo = new Mock<IRepository<User>>();
            mockUserRepo.Setup(m => m.FindOne(It.IsAny<Expression<Func<User, bool>>>())).Returns(existingUser);

            var mockRecordRepo = new Mock<IRepository<Record>>();
            mockRecordRepo.Setup(m => m.Add(It.Is<Record>(r => r == newRecord)));

            var controller = new RecordsController(mockUserRepo.Object, mockRecordRepo.Object);
            var result = controller.NewRecord(existingUser.DeviceId, newRecord);

            Assert.IsInstanceOfType<OkResult>(result);

            mockUserRepo.Verify();
            mockRecordRepo.Verify();
        }

        [TestMethod]
        public void NewRecord_InvalidUser_Test()
        {
            var mockUserRepo = new Mock<IRepository<User>>();
            var mockRecordsRepo = new Mock<IRepository<Record>>();
            var controller = new RecordsController(mockUserRepo.Object, mockRecordsRepo.Object);
            var result = controller.NewRecord(Guid.NewGuid().ToString(), new Record());
            Assert.IsInstanceOfType<NotFoundObjectResult>(result);
        }
    }
}
