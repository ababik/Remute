using System;

namespace Remutable.Tests.Model
{
    internal class Employee
    {
        public Guid Id { get; }

        public string FirstName { get; }

        public string LastName { get; }

        public Employee(Guid id, string firstName, string lastName)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
        }
    }

    internal class EmployeePoco
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}
