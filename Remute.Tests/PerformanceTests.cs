using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Remutable.Tests.Model;

namespace Remutable.Tests
{
    [TestClass]
    public class PerformanceTests
    {
        [TestMethod]
        public void SetFirstLevelProperyTest()
        {
            // ~13 ms

            var remute = new Remute();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            for (var i = 0; i < 1000; i++)
            {
                var organization = new Organization("organization 1", new Department("department 1", new Employee(Guid.NewGuid(), "developer", "manager"), null));
                var actual = remute.With(organization, x => x.Name, "organization 2");
            }

            stopwatch.Stop();

            var time = stopwatch.ElapsedMilliseconds;
            Console.WriteLine(time);
        }

        [TestMethod]
        public void SetNestedProperyTest()
        {
            // ~19 ms
            
            var remute = new Remute();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            for (var i = 0; i < 1000; i++)
            {
                var organization = new Organization("organization 1", new Department("department 1", new Employee(Guid.NewGuid(), "developer", "manager"), null));
                var actual = remute.With(organization, x => x.DevelopmentDepartment.Manager.FirstName, "name");
            }

            stopwatch.Stop();

            var time = stopwatch.ElapsedMilliseconds;
            Console.WriteLine(time);
        }

        [TestMethod]
        public void IndexTest()
        {
            // ~24 ms

            var employees = new Employee[1000];
            for (var i = 0; i < employees.Length; i++)
            {
                employees[i] = new Employee(Guid.NewGuid(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            }

            var department = new Department(null, null, employees);
            
            var remute = new Remute();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            for (var i = 0; i < employees.Length; i++)
            {
                var actual = remute.With(department, x => x.Employees[i], null);
            }

            stopwatch.Stop();

            var time = stopwatch.ElapsedMilliseconds;
            Console.WriteLine(time);
        }
    }
}