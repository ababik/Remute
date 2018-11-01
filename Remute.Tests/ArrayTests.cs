using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Remutable.Tests.Model;

namespace Remutable.Tests
{
    [TestClass]
    public class ArrayTests
    {
        [TestMethod]
        public void Test1()
        {
            var employees = new Employee[]
            {
                new Employee(Guid.NewGuid(), "Emp", "1"),
                new Employee(Guid.NewGuid(), "Emp", "2"),
                new Employee(Guid.NewGuid(), "Emp", "3"),
            };

            var department = new Department(null, null, employees);

            var employee1 = new Employee(Guid.NewGuid(), "Foo", "Bar");

            var actual = Remute.Default.With(department, x => x.Employees[1], employee1);

            Assert.AreNotSame(department, actual);
            Assert.AreSame(employees, department.Employees);
            Assert.AreSame(employees, actual.Employees);
            Assert.AreSame(employees[1], employee1);
            Assert.AreSame(employees[1], actual.Employees[1]);
            Assert.AreEqual(employees[1].FirstName, employee1.FirstName);
            Assert.AreEqual(employees[1].LastName, employee1.LastName);
        }

        [TestMethod]
        public void Test2()
        {
            var employees = new Employee[]
            {
                new Employee(Guid.NewGuid(), "Emp", "1"),
                new Employee(Guid.NewGuid(), "Emp", "2"),
                new Employee(Guid.NewGuid(), "Emp", "3"),
            };

            var department = new Department(null, null, employees);

            var actual = Remute.Default.With(department, x => x.Employees[1].LastName, "Foo");

            Assert.AreNotSame(department, actual);
            Assert.AreSame(department.Employees, actual.Employees);
            Assert.AreSame(department.Employees[0], actual.Employees[0]);
            Assert.AreSame(department.Employees[1], actual.Employees[1]);
            Assert.AreSame(department.Employees[2], actual.Employees[2]);

            Assert.AreEqual(department.Employees[1].FirstName, actual.Employees[1].FirstName);
            Assert.AreEqual(department.Employees[1].LastName, actual.Employees[1].LastName);

            Assert.AreEqual("Emp", actual.Employees[1].FirstName);
            Assert.AreEqual("Foo", actual.Employees[1].LastName);
        }

        [TestMethod]
        public void MultiLevelTest1()
        {
            var employees = new Employee[] 
            {
                new Employee(Guid.NewGuid(), "Joe", "Doe"),
                new Employee(Guid.NewGuid(), "1", "2")
            };
            var level3 = new Level3(employees);
            var level2 = new Level2(level3);
            var level1 = new Level1(level2);

            var actual = Remute.Default.With(level1, x => x.Level2.Level3.Employees[0].FirstName, "Foo");
            Remute.Default.With(level1, x => x.Level2.Level3.Employees[1].FirstName, "Foo");

            Assert.AreNotSame(level1, actual);
            Assert.AreNotSame(level1.Level2, actual.Level2);
            Assert.AreNotSame(level1.Level2.Level3, actual.Level2.Level3);
            Assert.AreSame(level1.Level2.Level3.Employees, actual.Level2.Level3.Employees);

            Assert.AreEqual("Foo", actual.Level2.Level3.Employees[0].FirstName);
            Assert.AreEqual("Doe", actual.Level2.Level3.Employees[0].LastName);
        }

        [TestMethod]
        public void MultiLevelTest2()
        {
            var level3 = new Level3(new Employee[] { new Employee(Guid.NewGuid(), "1", "2"), new Employee(Guid.NewGuid(), "Joe", "Doe") });
            var level4 = new Level4(new Level3[] { level3 });

            var actual = Remute.Default.With(level4, x => x.Level3s[0].Employees[1].LastName, "Foo");

            Assert.AreNotSame(level4, actual);
            Assert.AreSame(level4.Level3s, actual.Level3s);
            Assert.AreSame(level4.Level3s[0].Employees, actual.Level3s[0].Employees);
            Assert.AreEqual("Foo", actual.Level3s[0].Employees[1].LastName);
        }

        internal class Level1
        {
            public Level2 Level2 { get; }

            public Level1(Level2 level2)
            {
                Level2 = level2;
            }
        }

        internal class Level2
        {
            public Level3 Level3 { get; }

            public Level2(Level3 level3)
            {
                Level3 = level3;
            }
        }

        internal class Level3
        {
            public Employee[] Employees { get; }

            public Level3(Employee[] employees)
            {
                Employees = employees;
            }
        }

        internal class Level4
        {
            public Level3[] Level3s { get; }

            public Level4(Level3[] level3s)
            {
                Level3s = level3s;
            }
        }
    }
}