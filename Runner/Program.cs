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

            DatabaseContext.DbName = "PremadeDb";
            // DatabaseOperations.UpdateDb();
            Plotter.Init(@"C:\Users\bened\AppData\Local\Programs\Python\Python310\python.exe");
            //GenerateSomeVarietyPlots();
            AverageDailySongVarietyByHourPlot();
            
            Log.Information("finished");
        }

        public static void GenerateSomeVarietyPlots()
        {
            var from = new DateTime(2021, 01, 01, 18, 00, 00);
            var to = new DateTime(2021, 01, 31);
            while(from < to)
            {
                var image = Statistics.SongVarietyByHourPlot(from, from + TimeSpan.FromDays(1));
                File.WriteAllBytes($"variety_{from:yyyy.MM.dd}.png", image);
                from += TimeSpan.FromDays(1);
            }
            
        }

        public static void AverageDailySongVarietyByHourPlot()
        {
            var from = new DateTime(2021, 01, 01);
            var to = new DateTime(2021, 12, 31);
            var image = Statistics.AverageDailySongVarietyByHourPlot(from, to);
            File.WriteAllBytes("yearly_variety.png", image);
        }
    }
}
