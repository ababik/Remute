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
        public void SetFirstLevelPropery()
        {
            // ~9 ms

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
        public void SetNestedPropery()
        {
            // ~16 ms
            
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
    }
}