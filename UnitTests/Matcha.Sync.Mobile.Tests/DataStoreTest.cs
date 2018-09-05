using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Matcha.Sync.Mobile.Tests
{
    [TestClass]
    public class DataStoreTest
    {
        private IDataStore _instance;

        [TestInitialize]
        public void Init()
        {
            _instance = DataStore.Instance.Init("appid");
        }

        [TestMethod]
        public void Add_Should_Insert_Key_Data_And_ExpiryDate()
        {
            //Arrange
            var testKey = "test";
            var testValue = "data";

            //Act
            _instance.Add(testKey, testValue, TimeSpan.FromDays(30));

            //Assert
            Assert.IsTrue(_instance.Exists(testKey));
        }

        [TestMethod]
        public void Add_Generic_Should_Insert_Key_Data_And_ExpiryDate()
        {
            //Arrange
            var testKey = "test";
            var testValue = new List<string>() {"test"};

            //Act
            _instance.Add<List<string>>(testKey, testValue, TimeSpan.FromDays(30));

            //Assert
            Assert.IsTrue(_instance.Exists(testKey));
        }

        [TestMethod]
        public void Empty_Should_Delete_Key_Data_And_ExpiryDate()
        {
            //Arrange
            var testKey = "test";
            var testValue = new List<string>() { "test" };

            //Act
            _instance.Add<List<string>>(testKey, testValue, TimeSpan.FromDays(30));
            _instance.Empty(testKey);

            //Assert
            Assert.IsTrue(!_instance.Exists(testKey));
        }

        [TestMethod]
        public void EmptyAll_Should_DeleteALl_Key_Data_And_ExpiryDate()
        {
            //Arrange
            var testKey = "testKey";
            var testKey2 = "testKey2";
            var testValue = new List<string>() { "test" };

            //Act
            _instance.Add<List<string>>(testKey, testValue, TimeSpan.FromDays(30));
            _instance.Add(testKey2, "test", TimeSpan.FromDays(30));

            _instance.EmptyAll();

            //Assert
            Assert.IsTrue(!_instance.Exists(testKey));
            Assert.IsTrue(!_instance.Exists(testKey2));
        }

        [TestMethod]
        public void EmptyExpired_Should_Delete_Key_Data_And_ExpiryDate()
        {
            //Arrange
            var testKey = "test";
            var testValue = new List<string>() { "test" };

            //Act
            _instance.Add<List<string>>(testKey, testValue, TimeSpan.FromMilliseconds(0.00001));
            _instance.EmptyExpired();

            //Assert
            Assert.IsTrue(!_instance.Exists(testKey));
        }

        [TestMethod]
        public void Get_Should_Fetch_String()
        {
            //Arrange
            var testKey = "test";
            var expectedValue = "data";

            //Act
            _instance.Add(testKey, expectedValue, TimeSpan.FromDays(30));
            var data = _instance.Get(testKey);

            //Assert
            Assert.IsTrue(data == expectedValue);
        }

        [TestMethod]
        public void IsExpired_Should_Check_If_Expired()
        {
            //Arrange
            var testKey = "test";
            var testValue = new List<string>() { "test" };

            //Act
            _instance.Add<List<string>>(testKey, testValue, TimeSpan.FromMilliseconds(0.00001));
            var isExpired = _instance.IsExpired(testKey);

            //Assert
            Assert.IsTrue(isExpired);
        }

        [TestMethod]
        public void GetExpiration_Should_Get_The_Date_Of_Expiry()
        {
            //Arrange
            var testKey = "test";
            var testValue = new List<string>() { "test" };
            var expectedDate = DateTime.Now.AddDays(1);

            //Act
            _instance.Add<List<string>>(testKey, testValue, TimeSpan.FromDays(1));
            var date = _instance.GetExpiration(testKey);

            //Assert
            Assert.IsTrue(expectedDate.Date == date.Value.Date);
        }

        [TestMethod]
        public void GetLastUpdate_Should_Get_The_Date_Of_LastUpdate()
        {
            //Arrange
            var testKey = "test";
            var testValue = new List<string>() { "test" };
            var expectedDate = DateTime.Now;

            //Act
            _instance.Add<List<string>>(testKey, testValue, TimeSpan.FromDays(1));
            var date = _instance.GetLastUpdate(testKey);

            //Assert
            Assert.IsTrue(expectedDate.Date == date.Value.Date);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _instance.EmptyAll();
        }
    }
}
