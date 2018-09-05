using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Matcha.Sync.Mobile.Tests
{
    [TestClass]
    public class MobileServiceClient_IntegrationTest
    {
        private IMobileServiceClient _instance;

        [TestInitialize]
        public void Init()
        {
            //Warning:
            //We are using the staging api for integration test
            //So it is not guarantee to make all test pass (downtime)
            MobileServiceClient.Init("https://sampleapisync.azurewebsites.net/api");
            _instance = MobileServiceClient.Instance;
        }

        [TestMethod]
        public async Task PullAsync_Should_Be_Able_To_Pull_A_Data()
        {
            //Arrange
            var table = _instance.GetSyncTable<TodoItem>();

            //Act
            var query = table.CreateQuery().Where(e => e.Name.Contains("Task")).OrderBy(e=> e.Name);
            await table.PullAsync("pulltest", query);
            var data = table.ToList("pulltest");

            //Assert
            Assert.IsTrue(data.Count > 0);
            Assert.IsInstanceOfType(table, typeof(IMobileServiceCrudTable<TodoItem>));
        }

        [TestMethod]
        public async Task PullAsync_Should_Be_Able_To_Pull_A_Data_OrderBy_Desc()
        {
            //Arrange
            var table = _instance.GetSyncTable<TodoItem>();

            //Act
            var query = table.CreateQuery().Where(e => e.Name.Contains("Task")).OrderByDescending(e => e.Name);
            await table.PullAsync("pulltest", query);
            var data = table.ToList("pulltest");

            //Assert
            Assert.IsTrue(data.Count > 0);
            Assert.IsInstanceOfType(table, typeof(IMobileServiceCrudTable<TodoItem>));
        }

        [TestMethod]
        public async Task PushAsync_Should_Be_Able_To_Push_A_Data_To_The_Server()
        {
            //Arrange
            var table = _instance.GetSyncTable<TodoItem>();

            var item = new TodoItem
            {
                Id = 1000,
                Name = "Laundry Test Entry For Integration Test"
            };

            //Act
            table.InsertOrUpdate(item);

            await table.PushAsync();

            var resultSet = await table.ExecuteQuery(table.CreateQuery().Where(e => e.Name == "Laundry Test Entry For Integration Test"));

            //Assert (WHEN EXIST IN SERVER)
            Assert.IsTrue(resultSet.DataList.Any(e=> e.Name == "Laundry Test Entry For Integration Test"));


            table.Delete(item);
            await table.PushAsync();

            resultSet = await table.ExecuteQuery(table.CreateQuery().Where(e => e.Name == "Laundry Test Entry For Integration Test"));

            //Assert (WHEN DELETED IN SERVER)
            Assert.IsTrue(resultSet.DataList.All(e => e.Name != "Laundry Test Entry For Integration Test"));

            //Assert
            Assert.IsInstanceOfType(table, typeof(IMobileServiceCrudTable<TodoItem>));
        }

        [TestCleanup]
        public void Cleanup()
        {
        }
    }
}
