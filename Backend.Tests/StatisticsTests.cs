using Backend.Entities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Tests
{
    public class StatisticsTests : BaseTests
    {
        [Test]
        public void MostPlayedSongs()
        {
            // drop db
            using var db = new DatabaseContext(dropDb: true);
            // insert dummy event at from time
            var from = new DateTime(2021, 11, 01, 15, 00, 00);
            db.Events.Add(new()
            {
                StartTimeUnix = Backend.Util.UnixTimestamp(from),
                Song = new()
                {
                    PrimaryArtist = new Artist(),
                },
            });
            db.SaveChanges();


            var duration = TimeSpan.FromMinutes(5);
            var to = from + duration;
            DatabaseOperations.UpdateDb(to);


            // UpdateDb imports more than just 5 minutes so just take the stats from a whole day
            var topK = Statistics.GetMostPlayedSongs(from, from + TimeSpan.FromDays(1), 5);
            Assert.AreEqual(5, topK.Count);
            Assert.AreEqual(2, topK[0].Count);
            Assert.AreEqual(2, topK[1].Count);
        }
    }
}
