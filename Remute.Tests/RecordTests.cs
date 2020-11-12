using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Remutable.Tests.Model;

namespace Remutable.Tests
{
    [TestClass]
    public class RecordTests
    {
        [TestMethod]
        public void FirstLevelProperty()
        {
            var record = new Record1(Guid.NewGuid(), "Record 1");
            var title = "Record 2";
            var actual = Remute.Default.With(record, x => x.Title, title);
            Assert.AreEqual(record.Id, actual.Id);
            Assert.AreEqual(title, actual.Title);
        }

        [TestMethod]
        public void NestedProperty()
        {
            var record1 = new Record1(Guid.NewGuid(), "Record 1");
            var record2 = new Record2(record1, true);
            var id = Guid.NewGuid();
            var actual = Remute.Default.With(record2, x => x.Record1.Id, id);
            Assert.AreEqual(id, actual.Record1.Id);
        }
    }
}