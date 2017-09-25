namespace Remute.Tests.Model
{
    internal class Employee
    {
        public string FirstName { get; }

        public string LastName { get; }

        public Employee(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }
    }
}
