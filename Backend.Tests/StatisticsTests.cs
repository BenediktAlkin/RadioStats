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
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            // drop db
            using var db = new DatabaseContext(dropDb: true);
        }

        [Test]
        public void MostPlayedSongs()
        {
            var from = new DateTime(2021, 11, 01, 18, 00, 00);
            var till = from + TimeSpan.FromHours(12);
            DatabaseOperations.UpdateDb(from, till);

            var topKSongCounts = Statistics.GetMostPlayedSongs(from, till, 5);
            Assert.AreEqual(5, topKSongCounts.Count);
            Assert.AreEqual(2, topKSongCounts[0].Item2);
            Assert.AreEqual(2, topKSongCounts[1].Item2);
        }

        [Test]
        public void SongDiversity()
        {
            var from = new DateTime(2021, 11, 01, 0, 00, 00);
            var till = from + TimeSpan.FromDays(1);
            DatabaseOperations.UpdateDb(from, till);

            var variety = Statistics.SongVarietyByHour(from, till);
        }
        [Test]
        public void SongDiversityPlot()
        {
            var from = new DateTime(2021, 11, 03, 0, 00, 00);
            var till = from + TimeSpan.FromDays(1);
            DatabaseOperations.UpdateDb(from, till);

            Statistics.CreateSongVarietyByHourPlot(from, till);
        }

        [Test]
        public void SongCounts()
        {
            var from = new DateTime(2021, 11, 01, 0, 00, 00);
            var till = from + TimeSpan.FromHours(12);
            DatabaseOperations.UpdateDb(from, till);


            var uniqueSongCount = Statistics.UniqueSongCount(from, till);
            var totalSongCount = Statistics.TotalSongCount(from, till);
        }
    }
}
