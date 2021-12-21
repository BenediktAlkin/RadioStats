using Backend;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Tweetinvi;

namespace Tweeter
{
    public record TweeterServiceConfig(string ApiKey, string ApiSecretKey, string ApiAccessToken, string ApiAccessTokenSecret);

    public class TweeterService
    {
        private static readonly TimeSpan TWEET_INTERVAL = TimeSpan.FromMinutes(1);

        private TweeterServiceConfig Config { get; set; }
        private TwitterClient Client { get; set; }

        public delegate void TweeterServiceErrorEventHandler();
        public event TweeterServiceErrorEventHandler OnError;

        public TweeterService(TweeterServiceConfig config)
        {
            Config = config;

            // initialize twitter client and check connection
            var client = new TwitterClient(config.ApiKey, config.ApiSecretKey, config.ApiAccessToken, config.ApiAccessTokenSecret);
            var user = client.Users.GetAuthenticatedUserAsync().Result;
            Log.Information($"authenticated as {user.Name}");
        }


        public async Task Start(TimeSpan tweetTime)
        {
            // wait for next time to tweet
            if (tweetTime != default)
            {
                var now = DateTime.Now;
                var nextTweetTime = new DateTime(now.Year, now.Month, now.Day) + tweetTime;
                if (now > nextTweetTime)
                    nextTweetTime = nextTweetTime.AddDays(1);
                Log.Information($"next tweet time: {nextTweetTime}");
                await Task.Delay((int)(nextTweetTime - now).TotalMilliseconds);
            }

            // start timer that tweets regularly
            var timer = new Timer()
            {
                AutoReset = true,
                Interval = TWEET_INTERVAL.TotalMilliseconds,
            };
            timer.Elapsed += async (sender, e) => await UpdateDbAndTweet();
            timer.Enabled = true;
            // tweet right now (timer elapses the first time after the interval)
            await UpdateDbAndTweet();
        }

        private async Task UpdateDbAndTweet()
        {
            try
            {
                Log.Information("updating db");
                DatabaseOperations.UpdateDb();

                Log.Information("retrieving stats");
                var now = DateTime.Now;
                var mostPlayedSongs = Statistics.GetMostPlayedSongs(now.AddDays(-1), now, 5);
                var tweetStrings = mostPlayedSongs.Select(songCount => $"{songCount.Count}x {songCount.Object}");
                var tweetString = string.Join('\n', tweetStrings);


                var tweet = await Client.Tweets.PublishTweetAsync(tweetString);
                Log.Information($"published tweet: {tweet}");
                Log.Information($"next tweet time: {DateTime.Now + TWEET_INTERVAL}");
            }
            catch(Exception e)
            {
                Log.Error($"failed to UpdateDbAndTweet {e.Message}{Environment.NewLine}{e}");
                OnError();
                throw;
            }
        }
    }
}
