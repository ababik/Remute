namespace Remute.Tests.Model
{
    public class InvalidMultipleConstructor
    {
        public string Property1 { get; }

        public InvalidMultipleConstructor()
        {

        }

        public InvalidMultipleConstructor(string property1)
        {
            Property1 = property1;
        }
    }
}
