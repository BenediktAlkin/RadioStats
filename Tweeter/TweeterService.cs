using Backend;
using Serilog;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Tweetinvi;
using Tweetinvi.Parameters;

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
                var startTime = DateTime.Now;
                Log.Information("updating db");
                DatabaseOperations.UpdateDb();

                await TweetStatistics(DateTime.Now);
                
                Log.Information($"next tweet time: {startTime + TWEET_INTERVAL}");
            }
            catch(Exception e)
            {
                Log.Error($"failed to {nameof(UpdateDbAndTweet)} {e.Message}{Environment.NewLine}{e}");
                OnError();
                throw;
            }
        }
        public async Task TweetStatistics(DateTime to)
        {
            var from = to.AddDays(-1);

            // get tweet picture
            var image = Statistics.SongVarietyByHourPlot(from, to);
            var uploadedImage = await Client.Upload.UploadTweetImageAsync(image);
            // get tweet text
            var tweetString = GetStatisticsText(from, to);
            // publish tweet
            var tweet = await Client.Tweets.PublishTweetAsync(new PublishTweetParameters(tweetString)
            {
                Medias = { uploadedImage }
            });

            Log.Information($"published tweet:{Environment.NewLine}{tweet}");
        }
        private static string GetStatisticsText(DateTime from, DateTime to)
        {
            Log.Information("retrieving stats");
            var totalSongCount = Statistics.TotalSongCount(from, to);
            var totalSongMinutes = Statistics.TotalSongMinutes(from, to);
            var uniqueSongCount = Statistics.UniqueSongCount(from, to);
            var uniqueSongRatio = (int)Math.Round((double)uniqueSongCount * 100 / Math.Max(1, totalSongCount));
            var mostPlayedSongs = Statistics.MostPlayedSongs(from, to, 10);
            var tweetStrings = mostPlayedSongs.Select(songCount => $"{songCount.Item2}x {songCount.Item1}").ToList();

            var sb = new StringBuilder();
            // do this to avoid duplicates (duplicates are forbidden)
            sb.Append($"{to:MM.dd}\n");
            sb.Append($"{totalSongCount} gespielte Songs ({totalSongMinutes} Minuten)\n");
            sb.Append($"{uniqueSongCount} einzigartige Songs ({uniqueSongRatio}%)\n");
            
            if (tweetStrings.Count > 0)
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
            return sb.ToString();
        }
    }
}
