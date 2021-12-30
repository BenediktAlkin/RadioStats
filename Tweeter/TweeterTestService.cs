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
    public class TweeterTestService : TweeterService
    {
        public TweeterTestService(TweeterServiceConfig config) : base(config) { }


        public override Task TweetStatistics(DateTime to)
        {
            var from = to.AddDays(-1);

            // get tweet text
            var tweetString = GetStatisticsText(from, to);
            Log.Information($"TestTweet for {to:d}: {tweetString}");

            return Task.CompletedTask;
        }
    }
}
