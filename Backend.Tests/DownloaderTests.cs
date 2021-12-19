using NUnit.Framework;
using System;
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
        [TestCase("12341A12", "1234112")]
        public void TidyUpJson(string idValue, string expectedIdValue)
        {
            const string JSON_TEMPLATE = "[{{\"Id\":\"{0}\"}}]";
            var json = string.Format(JSON_TEMPLATE, idValue);
            var tidyJson = Util.InvokePrivateMethod<string>(typeof(Downloader), "TidyUpJson", json);
            var expected = string.Format(JSON_TEMPLATE, expectedIdValue);
            Assert.AreEqual(expected, tidyJson);
        }

        [Test]
        public void TidyUpJsonMulti()
        {
            var json = File.ReadAllText("Resources/TidyUpJsonMulti_input.json");
            json = Util.RemoveWhitespaceOutsideQuotes(json);
            var tidyJson = Util.InvokePrivateMethod<string>(typeof(Downloader), "TidyUpJson", json);
            var expected = File.ReadAllText("Resources/TidyUpJsonMulti_expected.json");
            Assert.AreEqual(Util.RemoveWhitespaceOutsideQuotes(expected), Util.RemoveWhitespaceOutsideQuotes(tidyJson));
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