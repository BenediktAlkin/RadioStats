using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Backend.Tests
{
    public class DownloaderTests : BaseTests
    {
        [Test]
        [TestCase("2021.12.19 18:00")]
        public void DownloadJson(string dateTimeStr)
        {
            var dateTime = DateTime.Parse(dateTimeStr);
            var json = Util.InvokePrivateMethod<string>(typeof(Downloader), "DownloadJson", dateTime);

            var expected = File.ReadAllText($"Resources/{dateTimeStr.Replace(':', '.')}.json");
            expected = Util.RemoveWhitespaceOutsideQuotes(expected);
            Assert.AreEqual(expected, json);
        }

        [Test]
        [TestCase("2021.12.15 18:00", 12)]
        public void DownloadJsonEvents(string dateTimeStr, int count)
        {
            var dateTime = DateTime.Parse(dateTimeStr);
            var jsonEvents = Util.InvokePrivateMethod<List<JsonEvent>>(typeof(Downloader), "DownloadJsonEvents", dateTime);

            Assert.AreEqual(count, jsonEvents.Count);
        }


        [Test]
        public void JsonToJsonEvent()
        {
            var json = File.ReadAllText("Resources/JsonToJsonEvent.json");
            json = Util.RemoveWhitespaceOutsideQuotes(json);
            var jsonEvents = Util.InvokePrivateMethod<JsonEvent[]>(typeof(Downloader), "JsonToJsonEvents", json);
            Assert.AreEqual(2, jsonEvents.Length);
        }
    }
}