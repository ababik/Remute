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
        public void Remute_Success()
        {
            var employee = new Employee(Guid.NewGuid(), "Joe", "Doe");

            var actual = employee
                .Remute(x => x.FirstName, "Foo")
                .Remute(x => x.LastName, "Bar");

            Assert.AreEqual("Foo", actual.FirstName);
            Assert.AreEqual("Bar", actual.LastName);
        }

        [TestMethod]
        public void CreateAndRemute_Success()
        {
            var employeePoco = new EmployeePoco() { Id = Guid.NewGuid(), FirstName = "Joe", LastName = "Doe" };
            var id = Guid.NewGuid();

            var actual = employeePoco
                .Remute<Employee>()
                .Remute(x => x.Id, id)
                .Remute(x => x.FirstName, "Foo")
                .Remute(x => x.LastName, "Bar");

            Assert.IsInstanceOfType(actual, typeof(Employee));
            Assert.AreEqual(id, actual.Id);
            Assert.AreEqual("Foo", actual.FirstName);
            Assert.AreEqual("Bar", actual.LastName);
        }
    }
}