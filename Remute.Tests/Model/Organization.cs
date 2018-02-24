namespace Remutable.Tests.Model
{
    internal class Organization
    {
        public string Name { get; }

        public Department DevelopmentDepartment { get; }

        public Organization(string name, Department developmentDepartment)
        {
            Name = name;
            DevelopmentDepartment = developmentDepartment;
        }
    }
}
