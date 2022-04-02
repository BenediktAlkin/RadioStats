using Backend.Entities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
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

            Plotter.Init(@"C:\Users\bened\AppData\Local\Programs\Python\Python310\python.exe");
            DatabaseContext.DbName = "Resources/TestDatabase";
        }

        [Test]
        public void MostPlayedSongs()
        {
            var from = new DateTime(2021, 11, 01, 18, 00, 00);
            var till = from + TimeSpan.FromHours(12);

            var mostPlayedSongResults = Statistics.MostPlayedSongs(from, till, 5);
            Assert.AreEqual(5, mostPlayedSongResults.TopK);
            Assert.AreEqual(2, mostPlayedSongResults.Counts[0]);
            Assert.AreEqual(2, mostPlayedSongResults.Counts[1]);
        }

        [Test]
        public void SongDiversity()
        {
            var from = new DateTime(2021, 11, 01, 0, 00, 00);
            var till = from + TimeSpan.FromDays(1);

            var variety = Statistics.SongVarietyByHour(from, till);
            // TODO asserts
        }
        [Test]
        public void SongVarietyPlotEmpty()
        {
            var from = new DateTime(2022, 11, 03, 0, 00, 00);
            var till = from + TimeSpan.FromDays(1);

            var image = Statistics.SongVarietyByHourPlot(from, till);
            var expected = File.ReadAllBytes("Resources/variety_empty.png");
            Assert.AreEqual(expected, image);
        }
        [Test]
        public void AverageDailySongVariety()
        {
            var from = new DateTime(2021, 01, 01);
            var to = new DateTime(2021, 12, 25);
            var image = Statistics.AverageDailySongVarietyByHourPlot(from, to);
        }
        [Test]
        public void SongVarietyOnlyUniqueSongs()
        {
            // on 30.12.2021 Ö3 played only unique songs
            var from = new DateTime(2021, 12, 30, 0, 00, 00);
            var till = from + TimeSpan.FromDays(1);

            var varieties = Statistics.SongVarietyByHour(from, till);
            Assert.IsTrue(varieties.All(v => v.Item2 == 1));
        }

        [Test]
        public void SongCounts()
        {
            var from = new DateTime(2021, 11, 01, 0, 00, 00);
            var to = from + TimeSpan.FromHours(12);

            var uniqueSongsResult = Statistics.GetUniqueSongs(from, to);
            // TODO asserts
        }

        [Test]
        public void WeeklyStats()
        {
            var from = new DateTime(2021, 08, 15, 18, 00, 00);
            for(var i = 0; i < 10; i++)
            {
                var to = from.AddDays(7);
                //var result = Statistics.GetTimeSpanStats(from, to);
                //Console.WriteLine($"{result.SongCount} {result.UniqueSongCount} {result.UniqueSongPercentage*100:N0} " +
                //    $"{result.Minutes} {result.MusicMinutes} {result.MusicMinutesPercent*100:N0}");
                from = to;
            }
        }

        [Test]
        public void MusicTime()
        {
            var from = new DateTime(2021, 08, 15, 18, 00, 00);
            for(var i = 0; i < 1000; i++)
            {
                var to = from.AddHours(1);
                var result = Statistics.GetMusicDuration(from, to);
                Console.WriteLine($"{from:HH} {result.Seconds} {result.MusicSeconds} {result.MusicPercentage * 100:N0}");
                from = to;
            }
        }
    }
}
