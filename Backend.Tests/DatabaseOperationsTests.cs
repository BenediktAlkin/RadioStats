using Backend.Entities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Tests
{
    public class DatabaseOperationsTests : BaseTests
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            // drop db
            using var db = new DatabaseContext(dropDb: true);
        }

        [Test]
        public void GetLatestEventDateTime()
        {
            using var db = new DatabaseContext();

            var datetime = DateTime.Now - TimeSpan.FromSeconds(10);
            db.Events.Add(new Event
            {
                StartTimeUnix = Backend.Util.UnixTimestamp(datetime),
                Song = new Song
                {
                    PrimaryArtist = new Artist(),
                },
            });
            db.SaveChanges();

            // DateTime.Now also has ms precision
            var expected = new DateTime(datetime.Year, datetime.Month, datetime.Day, datetime.Hour, datetime.Minute, datetime.Second);
            var actual = DatabaseOperations.GetLatestEventDateTime();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void UpdateDatabase_DoesNotThrowError()
        {
            DatabaseOperations.UpdateDb(Downloader.FIRST_DATE_WITH_DATA + TimeSpan.FromMinutes(5));
        }
    }
}
