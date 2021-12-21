using Backend;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Tweetinvi;
using YamlDotNet.Serialization;

namespace Tweeter
{
    public class Program
    {
        private static readonly TimeSpan TWEET_TIME = default;//new(23, 22, 00);
        private static readonly TimeSpan TWEET_TIME_DELTA = new(00, 00, 10);

        public static async Task Main()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("tweeter.log")
                .CreateLogger();

            Log.Information("updating db");
            DatabaseOperations.UpdateDb();

            // load credentials and authenticate twitter client
            var yaml = File.ReadAllText("credentials.yaml");
            var credentials = new DeserializerBuilder().Build().Deserialize<Credentials>(yaml);
            var client = new TwitterClient(credentials.ConsumerKey, credentials.ConsumerSecret, credentials.AccessToken, credentials.AccessTokenSecret);
            var user = await client.Users.GetAuthenticatedUserAsync();
            Log.Information($"authenticated as {user.Name}");


            // wait for next time to tweet
            if (TWEET_TIME != default)
            {
                var now = DateTime.Now;
                var nextTweetTime = new DateTime(now.Year, now.Month, now.Day, TWEET_TIME.Hours, TWEET_TIME.Minutes, TWEET_TIME.Seconds);
                if (now > nextTweetTime)
                    nextTweetTime = nextTweetTime.AddDays(1);
                Log.Information($"next tweet time: {nextTweetTime}");
                await Task.Delay((int)(nextTweetTime - now).TotalMilliseconds);
            }

            // start timer that tweets regularly
            var timer = new Timer()
            {
                AutoReset = true,
                Interval = TWEET_TIME_DELTA.TotalMilliseconds,
            };
            timer.Elapsed += (sender, e) => UpdateDbAndTweet(client);
            timer.Enabled = true;
            // tweet right now (timer elapses the first time after the interval)
            UpdateDbAndTweet(client);


            // wait indefinitely
            while (true)
                Console.ReadKey();
        }

        private static void UpdateDbAndTweet(ITwitterClient client)
        {
            Log.Information("updating db");
            DatabaseOperations.UpdateDb();

            Log.Information("retrieving stats");
            var now = DateTime.Now;
            var mostPlayedSongs = Statistics.GetMostPlayedSongs(now.AddDays(-1), now, 5);
            var tweetStrings = mostPlayedSongs.Select(songCount => $"{songCount.Count}x {songCount.Object}");
            var tweetString = string.Join('\n', tweetStrings);


            var tweet = client.Tweets.PublishTweetAsync(tweetString).Result;
            Log.Information($"published tweet: {tweet}");

            Log.Information($"next tweet time: {DateTime.Now + TWEET_TIME_DELTA}");
        }
    }
}
