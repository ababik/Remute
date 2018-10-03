namespace Remutable.Tests.Model
{
    public class PropertyMismatch
    {
        public string UserName { get; }

        public string NickName => "~" + UserName + "~";

        public PropertyMismatch(string userName)
        {
            UserName = userName;
        }
    }

    public class PropertyMismatchContainer
    {
        public PropertyMismatch PropertyMismatch { get; }

        public PropertyMismatchContainer(PropertyMismatch propertyMismatch)
        {
            PropertyMismatch = propertyMismatch;
        }
    }
}
