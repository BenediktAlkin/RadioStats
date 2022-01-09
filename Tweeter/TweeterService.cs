using Backend;
using Serilog;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Tweetinvi;
using Tweetinvi.Core.Models;
using Tweetinvi.Models;
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
        private IAuthenticatedUser User { get; set; }

        public delegate void TweeterServiceErrorEventHandler();
        public event TweeterServiceErrorEventHandler OnError;

        public TweeterService(TweeterServiceConfig config)
        {
            // initialize twitter client and check connection
            Client = new TwitterClient(config.ApiKey, config.ApiKeySecret, config.ApiAccessToken, config.ApiAccessTokenSecret);
            User = Client.Users.GetAuthenticatedUserAsync().Result;
            Log.Information($"authenticated as {User.Name}");
        }

        public async Task DeleteAllTweets()
        {
            var tweets = await Client.Timelines.GetUserTimelineAsync(User.Id);
            Log.Information($"found {tweets.Length} tweets in timeline to delete");
            for (var i = 0; i < tweets.Length; i++)
            {
                var tweet = tweets[i];
                if (tweet.FullText.Contains("Musikvielfalt Berechnung:"))
                    continue;
                await Client.Tweets.DestroyTweetAsync(tweet.Id);
                Log.Information($"deleted tweet {i+1}/{tweets.Length} {tweet.FullText}");
            }
        }

        public async Task MakePastTweets(TimeSpan tweetTime, DateTime startDate)
        {
            if (startDate == default) return;
            Log.Information($"making past tweets starting from {startDate:d}");
            var now = DateTime.Now;
            var today = new DateTime(now.Year, now.Month, now.Day);

            // set today as endDate
            DateTime endDate;
            var curTime = new TimeSpan(now.Hour, now.Minute, now.Second);
            if (curTime < tweetTime)
                // don't make tweet for today (it's generated automatically)
                endDate = today;
            else
                // make tweet for today (it is supposed to be already there)
                endDate = today + TimeSpan.FromDays(1);
            
            // make past tweets
            var curDate = new DateTime(startDate.Year, startDate.Month, startDate.Day, tweetTime.Hours, tweetTime.Minutes, tweetTime.Seconds);
            while(curDate < endDate)
            {
                await TweetStatistics(curDate);
                curDate += TimeSpan.FromDays(1);
            }
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
        public virtual async Task TweetStatistics(DateTime to)
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
            Log.Information($"published tweet for {to:d}:{Environment.NewLine}{tweet}");
        }
        protected static string GetStatisticsText(DateTime from, DateTime to)
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
            sb.Append($"{to:dd.MM}\n");
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
