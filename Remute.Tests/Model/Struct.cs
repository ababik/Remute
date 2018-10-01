namespace Remutable.Tests.Model
{
    public struct StructA
    {
        public bool Value { get; }

        public StructA(bool value)
        {
            Value = value;
        }
    }

    public struct StructB
    {
        public string Id { get; }

        public StructA StructA { get; }

        public StructB(string id, StructA structA)
        {
            Id = id;
            StructA = structA;
        }
    }
}
