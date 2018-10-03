using Microsoft.VisualStudio.TestTools.UnitTesting;
using Remutable.Tests.Model;
using System;

namespace Remutable.Tests
{
    [TestClass]
    public class PropertyMismatchTests
    {
        [TestMethod]
        public void SetFirstLevelPropery_Fail()
        {
            var expected = new PropertyMismatch("user1");
            var actual = default(PropertyMismatch);

            try
            {
                actual = Remute.Default.With(expected, x => x.NickName, "user2");
            }
            catch (Exception ex) when (ex.Message == $"Unable to construct object of type '{typeof(PropertyMismatch).Name}'. There is no constructor parameter matching property '{nameof(actual.NickName)}'.")
            {
                return;
            }

            Assert.Fail();
        }

        [TestMethod]
        public void SetNestedPropery_Fail()
        {
            var expected = new PropertyMismatchContainer(new PropertyMismatch("user1"));
            var actual = default(PropertyMismatchContainer);

            try
            {
                actual = Remute.Default.With(expected, x => x.PropertyMismatch.NickName, "user2");
            }
            catch (Exception ex) when (ex.Message == $"Unable to construct object of type '{typeof(PropertyMismatch).Name}'. There is no constructor parameter matching property '{nameof(actual.PropertyMismatch.NickName)}'.")
            {
                return;
            }

            Assert.Fail();
        }
    }
}
