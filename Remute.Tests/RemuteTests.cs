using Microsoft.VisualStudio.TestTools.UnitTesting;
using Remutable.Tests.Model;
using System;
using System.Linq.Expressions;

namespace Remutable.Tests
{
    [TestClass]
    public class RemuteTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InvalidInstanceParameter_ThrowsException()
        {
            var organization = default(Organization);
            var remute = new Remute();
            remute.With(organization, x => x.Name, "organization");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InvalidExpressionParameter_ThrowsException()
        {
            var organization = new Organization(null, null);
            var remute = new Remute();
            remute.With(organization, null, "organization");
        }

        [TestMethod]
        public void SetFirstLevelPropery_Success()
        {
            var organization = new Organization("organization 1", new Department("department 1", null, null));
            var remute = new Remute();
            var actual = remute.With(organization, x => x.Name, "organization 2");
            Assert.AreEqual("organization 2", actual.Name);
            Assert.AreNotSame(organization, actual);
            Assert.AreSame(organization.DevelopmentDepartment, actual.DevelopmentDepartment);
        }

        [TestMethod]
        public void SetNestedPropery_Success()
        {
            var organization = new Organization("organization 1", new Department("department 1", new Employee(Guid.NewGuid(), "developer", "manager"), null));
            var remute = new Remute();
            var actual = remute.With(organization, x => x.DevelopmentDepartment.Manager.FirstName, "name");
            Assert.AreEqual("name", actual.DevelopmentDepartment.Manager.FirstName);
            Assert.AreEqual(organization.DevelopmentDepartment.Manager.Id, actual.DevelopmentDepartment.Manager.Id);
            Assert.AreNotSame(organization, actual);
            Assert.AreNotSame(organization.DevelopmentDepartment, actual.DevelopmentDepartment);
            Assert.AreNotSame(organization.DevelopmentDepartment.Manager, actual.DevelopmentDepartment.Manager);
            Assert.AreSame(organization.Name, actual.Name);
            Assert.AreSame(organization.DevelopmentDepartment.Title, organization.DevelopmentDepartment.Title);
        }

        [TestMethod]
        public void SetValueTypePropery_Success()
        {
            var employeeId = Guid.NewGuid();
            var employee = new Employee(Guid.NewGuid(), "Joe", "Doe");
            var remute = new Remute();
            var actual = remute.With(employee, x => x.Id, employeeId);
            Assert.AreEqual(employeeId, actual.Id);
            Assert.AreEqual("Joe", actual.FirstName);
            Assert.AreEqual("Doe", actual.LastName);
            Assert.AreSame(employee.FirstName, actual.FirstName);
            Assert.AreSame(employee.LastName, actual.LastName);
        }

        [TestMethod]
        public void UnableToFindConstructor_ThrowsException()
        {
            var invalid = new InvalidMultipleConstructor();
            var remute = new Remute();
            try
            {
                invalid = remute.With(invalid, x => x.Property1, "test");
            }
            catch (Exception ex) when (ex.Message == $"Unable to find appropriate constructor of type '{nameof(InvalidMultipleConstructor)}'. Consider to use {nameof(ActivationConfiguration)} parameter.")
            {
                return;
            }

            Assert.Fail();
        }

        [TestMethod]
        public void UnableToFindProperty_ThrowsException()
        {
            var invalid = new InvalidProperty("property");
            var remute = new Remute();
            try
            {
                invalid = remute.With(invalid, x => x.Property1, "test");
            }
            catch (Exception ex) when (ex.Message == $"Unable to find appropriate property to use as a constructor parameter 'property'. Type '{nameof(InvalidProperty)}'. Consider to use {nameof(ActivationConfiguration)} parameter.")
            {
                return;
            }

            Assert.Fail();
        }

        [TestMethod]
        public void FiledSpecified_ThrowsException()
        {
            var invalid = new InvalidProperty("property");
            var remute = new Remute();
            try
            {
                invalid = remute.With(invalid, x => x.Field1, "test");
            }
            catch (Exception ex) when (ex.Message == $"Type member '{nameof(invalid.Field1)}' is expected to be a property.")
            {
                return;
            }

            Assert.Fail();
        }
    }
}
