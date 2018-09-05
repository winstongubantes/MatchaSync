using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Matcha.Sync.Mobile.Tests
{
    [TestClass]
    public class MobileServiceClientTest
    {
        private IMobileServiceClient _instance;

        [TestInitialize]
        public void Init()
        {
            MobileServiceClient.Init("test");
            _instance = MobileServiceClient.Instance;
        }

        [TestMethod]
        public void GetSyncTable_Should_Get_Generic_IMobileServiceCrudTable()
        {
            //Arrange
            var table = _instance.GetSyncTable<TestClass>();

            //Act

            //Assert
            Assert.IsInstanceOfType(table, typeof(IMobileServiceCrudTable<TestClass>));
        }

        [TestMethod]
        public void ToList_Should_Get_Generic_List_Of_Data()
        {
            //Arrange
            var table = _instance.GetSyncTable<TestClass>();

            //Act
            var list = table.ToList();

            //Assert
            Assert.IsInstanceOfType(list, typeof(IList<TestClass>));
        }

        [TestMethod]
        public void ToList_QueryID_Should_Get_Generic_List_Of_Data()
        {
            //Arrange
            var table = _instance.GetSyncTable<TestClass>();

            //Act
            var list = table.ToList("query");

            //Assert
            Assert.IsInstanceOfType(list, typeof(IList<TestClass>));
        }


        [TestMethod]
        public void InsertOrUpdate_Should_Insert_TestClass_Object()
        {
            //Arrange
            var table = _instance.GetSyncTable<TestClass>();
            var expectedName = "Test";

            //Act
            table.InsertOrUpdate(new TestClass
            {
                Name = "Test"
            });

            var list = table.ToList();

            //Assert
            Assert.IsTrue(list.Any(e=> e.Name == expectedName));
            Assert.IsInstanceOfType(list, typeof(IList<TestClass>));
        }

        [TestMethod]
        public void InsertOrUpdate_Should_Update_TestClass_Object()
        {
            //Arrange
            var table = _instance.GetSyncTable<TestClass>();
            var initialName = "InitialName";
            var expectedName = "TestUpdate";

            //Act
            table.InsertOrUpdate(new TestClass
            {
                Name = "InitialName"
            });

            var list = table.ToList();
            var record = list.FirstOrDefault(e=> e.Name == initialName);

            record.Name = "TestUpdate";
            table.InsertOrUpdate(record);

            //Assert
            Assert.IsTrue(table.ToList().Any(e => e.Name == expectedName));
            Assert.IsInstanceOfType(list, typeof(IList<TestClass>));
        }

        [TestMethod]
        public void Delete_Should_Tag_as_Delete_TestClass_Object()
        {
            //Arrange
            var table = _instance.GetSyncTable<TestClass>();
            var expectedName = "DeletedTest";

            //Act
            table.InsertOrUpdate(new TestClass
            {
                Name = "DeletedTest"
            });

            var list = table.ToList();
            var record = list.FirstOrDefault(e => e.Name == "DeletedTest");

            table.Delete(record);
            list = table.ToList();

            //Assert
            Assert.IsTrue(list.Any(e => e.Name == expectedName && e.IsDeleted));
            Assert.IsInstanceOfType(list, typeof(IList<TestClass>));
        }

        [TestMethod]
        public void CreateQuery_Should_Be_Able_Create_OData_Query_String()
        {
            //Arrange
            var table = _instance.GetSyncTable<TestClass>();
            var expectedQuery = "&$skip=10?$top=10&$count=true";

            //Act
            var query = table.CreateQuery().Take(10).Skip(10);

            //Assert
            Assert.IsTrue(expectedQuery == query.Query);
            Assert.IsInstanceOfType(query, typeof(IMobileServiceTableQuery<TestClass>));
        }

        [TestMethod]
        public void CreateQuery_Should_Be_Able_Create_OData_Filter_By_Contains_Name()
        {
            //Arrange
            var table = _instance.GetSyncTable<TestClass>();
            var expectedQuery = "?$filter=contains(Name , 'test')&$count=true";

            //Act
            var query = table.CreateQuery().Where(e=> e.Name.Contains("test"));

            //Assert
            Assert.IsTrue(expectedQuery == query.Query);
            Assert.IsInstanceOfType(query, typeof(IMobileServiceTableQuery<TestClass>));
        }

        [TestMethod]
        public void CreateQuery_Should_Be_Able_Create_OData_Filter_By_Contains_Name_Combine_With_Top_Skip()
        {
            //Arrange
            var table = _instance.GetSyncTable<TestClass>();
            var expectedQuery = "?$filter=contains(Name , 'test')&$skip=10?$top=10&$count=true";

            //Act
            var query = table.CreateQuery().Take(10).Skip(10).Where(e => e.Name.Contains("test"));

            //Assert
            Assert.IsTrue(expectedQuery == query.Query);
            Assert.IsInstanceOfType(query, typeof(IMobileServiceTableQuery<TestClass>));
        }

        [TestMethod]
        public void CreateQuery_Should_Be_Able_Create_OData_With_Multiple_Filter()
        {
            //Arrange
            var table = _instance.GetSyncTable<TestClass>();
            var expectedQuery = "?$filter=contains(Name , 'test') and startswith(Name , 't')&$skip=10?$top=10&$count=true";

            //Act
            var query = table.CreateQuery().Take(10).Skip(10)
                .Where(e => e.Name.Contains("test"))
                .Where(e => e.Name.StartsWith("t"));

            //Assert
            Assert.IsTrue(expectedQuery == query.Query);
            Assert.IsInstanceOfType(query, typeof(IMobileServiceTableQuery<TestClass>));
        }

        [TestMethod]
        public void CreateQuery_Should_Be_Able_Create_OData_With_Multiple_Filter_With_OR()
        {
            //Arrange
            var table = _instance.GetSyncTable<TestClass>();
            var expectedQuery = "?$filter=contains(Name , 'test') or startswith(Name , 't')&$skip=10?$top=10&$count=true";

            //Act
            var query = table.CreateQuery().Take(10).Skip(10)
                .Where(e => e.Name.Contains("test") || e.Name.StartsWith("t"));

            //Assert
            Assert.IsTrue(expectedQuery == query.Query);
            Assert.IsInstanceOfType(query, typeof(IMobileServiceTableQuery<TestClass>));
        }

        [TestMethod]
        public void CreateQuery_Should_Be_Able_Create_OData_With_Multiple_Filter_With_OR_And_With_Not()
        {
            //Arrange
            var table = _instance.GetSyncTable<TestClass>();
            var expectedQuery = "?$filter=not(contains(Name , 'test')) or startswith(Name , 't')&$skip=10?$top=10&$count=true";

            //Act
            var query = table.CreateQuery().Take(10).Skip(10)
                .Where(e => !e.Name.Contains("test") || e.Name.StartsWith("t"));

            //Assert
            Assert.IsTrue(expectedQuery == query.Query);
            Assert.IsInstanceOfType(query, typeof(IMobileServiceTableQuery<TestClass>));
        }


        [TestCleanup]
        public void Cleanup()
        {
        }
    }
}
