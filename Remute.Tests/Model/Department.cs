using System.Collections.Immutable;

namespace Remutable.Tests.Model
{
    internal class Department
    {
        public string Title { get; }

        public Employee Manager { get; }

        public ImmutableList<Employee> Employees { get; }

        public Department(string title, Employee manager, ImmutableList<Employee> employees)
        {
            Title = title;
            Manager = manager;
            Employees = employees;
        }
    }
}
