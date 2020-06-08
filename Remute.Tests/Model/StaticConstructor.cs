namespace Remutable.Tests.Model
{
    public class StaticConstructor
    {
        public string Property1 { get; }

        static StaticConstructor()
        {

        }

        public StaticConstructor(string property1)
        {
            Property1 = property1;
        }
    }

    public class StaticFieldInit
    {
        public string Property1 { get; }

        public static StaticFieldInit Default = new StaticFieldInit("Default");

        public StaticFieldInit(string property1)
        {
            Property1 = property1;
        }
    }
}
