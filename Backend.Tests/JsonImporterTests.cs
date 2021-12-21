using Backend.Entities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Tests
{
    public class JsonImporterTests : BaseTests
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            // drop db
            using var db = new DatabaseContext(dropDb: true);
        }


        [Test]
        [TestCase("2021.12.13 18:00")]
        public void ImportJsonEvents(string dateTimeStr)
        {
            var dateTime = DateTime.Parse(dateTimeStr);
            var jsonEvents = Downloader.DownloadJsonEvents(dateTime);

            JsonImporter.ImportJsonEvents(jsonEvents);

            using var db = new DatabaseContext();
            Assert.AreEqual(12, db.Events.Count());
        }
    }
}
