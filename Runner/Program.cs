using Backend;
using Serilog;
using System;

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

            DatabaseOperations.UpdateDb();

            Log.Information("finished");
        }
    }
}
