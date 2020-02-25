using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Remutable.Tests
{
    [TestClass]
    public class InheritedPropertiesTests
    {
        [TestMethod]
        public void TestBaseInheritedProperty1_Success()
		{
			var instance = new InheritedType4("prop1", "prop2", "prop3", "prop4");
			var actual = Remute.Default.With(instance, x => x.Prop1, "update");
			Assert.AreEqual("update", actual.Prop1);
			Assert.AreEqual(instance.Prop2, actual.Prop2);
			Assert.AreEqual(instance.Prop3, actual.Prop3);
			Assert.AreEqual(instance.Prop4, actual.Prop4);
		}

		[TestMethod]
        public void TestBaseInheritedProperty2_Success()
		{
			var instance = new InheritedType4("prop1", "prop2", "prop3", "prop4");
			var actual = Remute.Default.With(instance, x => x.Prop4, "update");
			Assert.AreEqual("update", actual.Prop4);
			Assert.AreEqual(instance.Prop1, actual.Prop1);
			Assert.AreEqual(instance.Prop2, actual.Prop2);
			Assert.AreEqual(instance.Prop3, actual.Prop3);
		}

		[TestMethod]
        public void TestConfig_Success()
		{
			var config = new ActivationConfiguration()
				.Configure<InheritedType4>(x => new InheritedType4(x.Prop1, x.Prop2, x.Prop3, x.Prop4));
			var remute = new Remute(config);
			var instance = new InheritedType4("prop1", "prop2", "prop3", "prop4");
			var actual = remute.With(instance, x => x.Prop1, "update");
			Assert.AreEqual("update", actual.Prop1);
			Assert.AreEqual(instance.Prop2, actual.Prop2);
			Assert.AreEqual(instance.Prop3, actual.Prop3);
			Assert.AreEqual(instance.Prop4, actual.Prop4);
		}
    }
}