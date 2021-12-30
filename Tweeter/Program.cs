using Backend;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Tweetinvi;
using YamlDotNet.Serialization;

namespace Tweeter
{
    public class Config
    {
        public string TargetEmail { get; set; }
        public TimeSpan TweetTime { get; set; }
        public DateTime StartDate { get; set; }

        public string PythonPath { get; set; }
        public bool IsTestRun { get; set; }
        public bool TestCredentials { get; set; }
    }

    public class Program
    {
        public static async Task Main()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("tweeter.log")
                .CreateLogger();

            var deserializer = new DeserializerBuilder().Build();

            // load configs
            var mailerYaml = File.ReadAllText("mailer_config.yaml");
            var mailerConfig = deserializer.Deserialize<MailerServiceConfig>(mailerYaml);
            var tweeterYaml = File.ReadAllText("tweeter_config.yaml");
            var tweeterConfig = deserializer.Deserialize<TweeterServiceConfig>(tweeterYaml);
            var configYaml = File.ReadAllText("config.yaml");
            var config = deserializer.Deserialize<Config>(configYaml);

            // setup mailer
            var mailer = new MailerService(mailerConfig);

            // setup tweeter
            TweeterService tweeter;
            if (config.IsTestRun)
                tweeter = new TweeterTestService(tweeterConfig);
            else
                tweeter = new TweeterService(tweeterConfig);

            // setup program
            Plotter.Init(config.PythonPath);
            tweeter.OnError += () => mailer.SendMail(config.TargetEmail, "Ö3RadioStats Error", "Encountered error in Ö3RadioStats");

            
            // do test run if specified
            if (config.TestCredentials)
            {
                // test credentials for mailer & twitter
                mailer.SendMail(config.TargetEmail, "Ö3RadioStats Test", "Test");
                try
                {
                    // test tweet (duplicates are blocked)
                    var range = (int)(DateTime.MaxValue - DateTime.MinValue).TotalDays;
                    var randomDate = DateTime.MinValue + TimeSpan.FromDays(new Random().Next(0, range));
                    await tweeter.TweetStatistics(randomDate);
                    Task.Delay(1000).Wait();
                }
                catch (Exception e)
                {
                    Log.Error($"failed to tweet {Environment.NewLine}{e.Message}");
                    Task.Delay(1000).Wait();
                }
                
                return;
            }


            // start program
            Log.Information("updating db");
            await tweeter.MakePastTweets(config.TweetTime, config.StartDate);
            DatabaseOperations.UpdateDb();
            await tweeter.Start(config.TweetTime, config.StartDate);
            
            

            // wait indefinitely
            while (true)
                Console.ReadKey();
        }

        
    }
}
