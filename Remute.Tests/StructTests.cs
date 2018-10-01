using Microsoft.VisualStudio.TestTools.UnitTesting;
using Remutable.Tests.Model;
using Remutable.Extensions;
using System;

namespace Remutable.Tests
{
    [TestClass]
    public class StructTests
    {
        [TestMethod]
        public void SetFirstLevelProperty_Success()
        {
            var structA = new StructA();
            var actual = structA.Remute(x => x.Value, true);
            Assert.AreNotSame(structA, actual);
            Assert.AreEqual(true, actual.Value);
        }

        [TestMethod]
        public void SetNestedProperty_Success()
        {
            var structB = new StructB(Guid.NewGuid().ToString(), new StructA(true));
            var actual = structB.Remute(x => x.StructA.Value, false);
            Assert.AreNotSame(structB, actual);
            Assert.AreEqual(false, actual.StructA.Value);
        }
    }
}
