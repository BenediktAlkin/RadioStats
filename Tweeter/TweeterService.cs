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
    public record TweeterServiceConfig
    {
        public string ApiKey { get; set; }
        public string ApiKeySecret { get; set; }
        public string ApiAccessToken { get; set; }
        public string ApiAccessTokenSecret { get; set; }
    }

    public class TweeterService
    {
        private static readonly TimeSpan TWEET_INTERVAL = TimeSpan.FromDays(1);
        private const int TWEET_MAX_CHARS = 280;

        private TwitterClient Client { get; set; }

        public delegate void TweeterServiceErrorEventHandler();
        public event TweeterServiceErrorEventHandler OnError;

        public TweeterService(TweeterServiceConfig config)
        {
            // initialize twitter client and check connection
            Client = new TwitterClient(config.ApiKey, config.ApiKeySecret, config.ApiAccessToken, config.ApiAccessTokenSecret);
            var user = Client.Users.GetAuthenticatedUserAsync().Result;
            Log.Information($"authenticated as {user.Name}");
        }

        public async Task Tweet(string message)
        {
            await Client.Tweets.PublishTweetAsync(message);
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
                var to = DateTime.Now;
                var from = to.AddDays(-1);
                var totalSongCount = Statistics.TotalSongCount(from, to);
                var totalSongMinutes = Statistics.TotalSongMinutes(from, to);
                var uniqueSongCount = Statistics.UniqueSongCount(from, to);
                var uniqueSongRatio = (int)Math.Round((double)uniqueSongCount / totalSongCount);
                var mostPlayedSongs = Statistics.GetMostPlayedSongs(from, to, 5);
                var tweetStrings = mostPlayedSongs.Select((song, count) => $"{count}x {song}").ToList();

                var sb = new StringBuilder();
                // do this to avoid duplicates (duplicates are forbidden)
                sb.Append($"{to:MM.dd}\n");
                sb.Append($"{totalSongCount} gespielte Songs ({totalSongMinutes} Minuten)\n");
                sb.Append($"{uniqueSongCount} einzigartige Songs ({uniqueSongRatio}%)\n");
                sb.Append($"Toptracks:");
                var i = 0;
                while (i < tweetStrings.Count)
                {
                    var curString = tweetStrings[i];
                    if (sb.Length + curString.Length < TWEET_MAX_CHARS)
                    {
                        sb.Append('\n');
                        sb.Append(curString);
                        i++;
                    }
                    else
                        break;
                }

                

                var tweet = await Client.Tweets.PublishTweetAsync(sb.ToString());
                Log.Information($"published tweet: {tweet}");
                Log.Information($"next tweet time: {DateTime.Now + TWEET_INTERVAL}");
                OnError();
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
