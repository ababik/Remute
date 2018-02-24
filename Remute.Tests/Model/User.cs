namespace Remutable.Tests.Model
{
    internal class User
    {
        public int Id { get; }
        public string FirstName { get; }
        public string LastName { get; }

        public User(int id, string firstName, string lastName)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
        }

        public User(string firstNameNotMatchingPropertyName, string lastName)
        {
            FirstName = firstNameNotMatchingPropertyName;
            LastName = lastName;
        }
    }
}
