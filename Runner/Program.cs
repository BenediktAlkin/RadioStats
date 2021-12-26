using Backend;
using Serilog;
using System;
using System.IO;

namespace Runner
{
    public class Program
    {
        public static void Main()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("runner.log")
                .CreateLogger();

            // DatabaseOperations.UpdateDb();
            var from = new DateTime(2021, 01, 01);
            var to = new DateTime(2021, 12, 25);
            var image = Statistics.AverageDailySongVarietyByHourPlot(from, to);
            File.WriteAllBytes("yearly_variety.png", image);
            
            Log.Information("finished");
        }
    }
}
