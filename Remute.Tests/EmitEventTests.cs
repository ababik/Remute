using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Remutable.Tests.Model;

namespace Remutable.Tests
{
    [TestClass]
    public class EmitEventTests
    {
        [TestMethod]
        public void EmitEventFiredWithProps_Success()
        {
            var expectedSource = default(Organization);
            var expectedTarget = default(Organization);
            var expectedValue = default(string);
            var expectedAffectedProperties = default(string[]);

            var remute = new Remute();
            remute.OnEmit += (source, target, value, affectedProperties) =>
            {
                expectedSource = (Organization)source;
                expectedTarget = (Organization)target;
                expectedValue = (string)value;
                expectedAffectedProperties = affectedProperties;
            };

            var organization = new Organization("organization 1", new Department("department 1", new Employee(Guid.NewGuid(), "developer", "manager"), null));
            var actual = remute.With(organization, x => x.DevelopmentDepartment.Manager.FirstName, "Foo");

            Assert.AreSame(expectedSource, organization);
            Assert.AreSame(expectedTarget, actual);
            Assert.AreEqual("Foo", expectedValue);
            Assert.IsTrue(expectedAffectedProperties.Length == 3);
            Assert.AreEqual(expectedAffectedProperties[0], "DevelopmentDepartment");
            Assert.AreEqual(expectedAffectedProperties[1], "Manager");
            Assert.AreEqual(expectedAffectedProperties[2], "FirstName");
        }

        [TestMethod]
        public void EmitEventFiredWithoutProps_Success()
        {
            var expectedSource = default(Employee);
            var expectedTarget = default(Employee);
            var expectedValue = default(string);
            var expectedAffectedProperties = default(string[]);

            var remute = new Remute();
            remute.OnEmit += (source, target, value, affectedProperties) =>
            {
                expectedSource = (Employee)source;
                expectedTarget = (Employee)target;
                expectedValue = (string)value;
                expectedAffectedProperties = affectedProperties;
            };

            var employee = new Employee(Guid.NewGuid(), "Joe", "Doe");
            var actual = remute.With<Employee>(employee);

            Assert.AreSame(expectedSource, employee);
            Assert.AreSame(expectedTarget, actual);
            Assert.IsNull(expectedValue);
            Assert.IsNull(expectedAffectedProperties);
        }
    }
}