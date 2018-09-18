using Microsoft.VisualStudio.TestTools.UnitTesting;
using Remutable.Extensions;
using Remutable.Tests.Model;
using System;

namespace Remutable.Tests
{
    [TestClass]
    public class ExtensionMethodTests
    {
        [TestMethod]
        public void Test()
        {
            var employee = new Employee(Guid.NewGuid(), "Joe", "Doe");

            var actual = employee
                .Remute(x => x.FirstName, "Foo")
                .Remute(x => x.LastName, "Bar");

            Assert.AreEqual("Foo", actual.FirstName);
            Assert.AreEqual("Bar", actual.LastName);
        }
    }
}