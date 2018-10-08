using Microsoft.VisualStudio.TestTools.UnitTesting;
using Remutable.Tests.Model;
using System;

namespace Remutable.Tests
{
    [TestClass]
    public class CreateTests
    {
        [TestMethod]
        public void Clone_Success()
        {
            var remute = new Remute();
            var expected = new Employee(Guid.NewGuid(), "Joe", "Doe");
            var actual = remute.With<Employee>(expected);

            Assert.IsInstanceOfType(actual, typeof(Employee));
            Assert.AreNotSame(expected, actual);
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.FirstName, actual.FirstName);
            Assert.AreEqual(expected.LastName, actual.LastName);
        }

        [TestMethod]
        public void FromPoco_Success()
        {
            var remute = new Remute();
            var expected = new EmployeePoco() { Id = Guid.NewGuid(), FirstName = "Joe", LastName = "Doe" };
            var actual = remute.With<Employee>(expected);

            Assert.IsInstanceOfType(actual, typeof(Employee));
            Assert.AreNotSame(expected, actual);
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.FirstName, actual.FirstName);
            Assert.AreEqual(expected.LastName, actual.LastName);
        }

        [TestMethod]
        public void FromAnonymousType_Success()
        {
            var remute = new Remute();
            var expected = new { Id = Guid.NewGuid(), FirstName = "Joe", LastName = "Doe" };
            var actual = remute.With<Employee>(expected);

            Assert.IsInstanceOfType(actual, typeof(Employee));
            Assert.AreNotSame(expected, actual);
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.FirstName, actual.FirstName);
            Assert.AreEqual(expected.LastName, actual.LastName);
        }

        [TestMethod]
        public void FromDynamic_Success()
        {
            var remute = new Remute();
            dynamic expected = new { Id = Guid.NewGuid(), FirstName = "Joe", LastName = "Doe" };
            var actual = remute.With<Employee>(expected);

            Assert.IsInstanceOfType(actual, typeof(Employee));
            Assert.AreNotSame(expected, actual);
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.FirstName, actual.FirstName);
            Assert.AreEqual(expected.LastName, actual.LastName);
        }
    }
}