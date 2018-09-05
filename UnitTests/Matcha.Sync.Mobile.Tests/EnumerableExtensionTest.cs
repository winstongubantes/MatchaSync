using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Matcha.Sync.Mobile.Tests
{
    [TestClass]
    public class EnumerableExtensionTest
    {
        [TestMethod]
        public void ForEach_Should_Iterate_Each_Record()
        {
            //Arrange
            var testValue = new List<TestClass>() { new TestClass {Name = "test1"}};
            var expectedValue = new List<TestClass>() { new TestClass { Name = "TEST1" } };

            //Act
            testValue.ForEach(e=> e.Name = e.Name.ToUpper());

            //Assert
            Assert.AreEqual(expectedValue[0].Name,testValue[0].Name);
        }
    }
}
