namespace Remutable.Tests.Model
{
    public class InvalidProperty
    {
        public string Property1 { get; }

        public string Field1;

        public InvalidProperty(string property)
        {
            Property1 = property;
        }
    }
}
