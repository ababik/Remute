using Microsoft.VisualStudio.TestTools.UnitTesting;
using Remutable.Extensions;

namespace Remutable.Tests
{
    [TestClass]
    public class InheritedPropertiesTests
    {
        [TestMethod]
        public void RemuteInheritance()
		{
			var bar = new Bar("1", "2");

			// throws System.InvalidCastException : Unable to cast object of type 'Tests.Foo' to type 'Tests.Bar'
            bar = Remute.Default.With(bar, x => x.A, "3");
			//bar = bar.Remute(b => b.A, "3"); 
			Assert.AreEqual("3", bar.A);
		}

        [TestMethod]
        public void RemuteInheritanceAttemptedWorkaround()
		{
			var bar = new Bar("1", "2");
			var config = new ActivationConfiguration()
				.Configure<Bar>(b => new Bar(b.A, b.B));
			var remute = new Remute(config);

			// throws System.Exception : Invalid property 'A'. Must be a member of 'Tests.Bar'
			bar = bar.Remute(b => b.A, "3", remute); 
			
			Assert.AreEqual("3", bar.A);
		}
    }

    public class Foo
	{
		public Foo(string a)
		{
			A = a;
		}

		public string A { get; }
	}
	
	public class Bar : Foo
	{
		public Bar(string a, string b) : base(a)
		{
			B = b;
		}

		public string B { get; }
	}
}