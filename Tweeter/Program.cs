using System;
using System.IO;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;
using YamlDotNet.Serialization;

namespace Tweeter
{
    public class Program
    {
        public static async Task Main()
        {
            var yaml = File.ReadAllText("credentials.yaml");
            var credentials = new DeserializerBuilder().Build().Deserialize<Credentials>(yaml);
            


            var userClient = new TwitterClient(
                credentials.ConsumerKey, 
                credentials.ConsumerSecret, 
                credentials.AccessToken, 
                credentials.AccessTokenSecret);

            var user = await userClient.Users.GetAuthenticatedUserAsync();
            Console.WriteLine("Hello " + user);


            var tweet = await userClient.Tweets.PublishTweetAsync("Hello tweetinvi world!");
        }
    }
}
