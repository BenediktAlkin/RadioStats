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
    public record Config(string TargetEmail, TimeSpan TweetTime);

    public class Program
    {
        public static async Task Main()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("tweeter.log")
                .CreateLogger();
            var deserializer = new DeserializerBuilder().Build();

            // setup mailer
            var mailerYaml = File.ReadAllText("mailer_config.yaml");
            var mailerConfig = deserializer.Deserialize<MailerServiceConfig>(mailerYaml);
            var mailer = new MailerService(mailerConfig);

            // setup tweeter
            var tweeterYaml = File.ReadAllText("tweeter_config.yaml");
            var tweeterConfig = deserializer.Deserialize<TweeterServiceConfig>(tweeterYaml);
            var tweeter = new TweeterService(tweeterConfig);

            // setup program
            var configYaml = File.ReadAllText("config.yaml");
            var config = deserializer.Deserialize<Config>(configYaml);
            tweeter.OnError += () => mailer.SendErrorMail(config.TargetEmail, "Ö3RadioStats Error", "Encountered error in Ö3RadioStats");


            // start program
            Log.Information("updating db");
            DatabaseOperations.UpdateDb();
            await tweeter.Start(config.TweetTime);
            
            

            // wait indefinitely
            while (true)
                Console.ReadKey();
        }

        
    }
}
