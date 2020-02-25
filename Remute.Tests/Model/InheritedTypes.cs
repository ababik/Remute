namespace Remutable.Tests
{
    public abstract class InheritedType1
    {
        public string Prop1 { get; }

        public InheritedType1(string prop1)
        {
            Prop1 = prop1;
        }
    }

    public abstract class InheritedType2 : InheritedType1
    {
        public string Prop2 { get; }

        public InheritedType2(string prop1, string prop2) : base(prop1)
        {
            Prop2 = prop2;
        }
    }

    public class InheritedType3 : InheritedType2
    {
        public string Prop3 { get; }

        public InheritedType3(string prop1, string prop2, string prop3) : base(prop1, prop2)
        {
            Prop3 = prop3;
        }
    }

    public class InheritedType4 : InheritedType3
    {
        public string Prop4 { get; }

        public InheritedType4(string prop1, string prop2, string prop3, string prop4) : base(prop1, prop2, prop3)
        {
            Prop4 = prop4;
        }
    }
}