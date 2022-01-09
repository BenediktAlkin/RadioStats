using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Backend
{
    public class Plotter
    {
        public static Plotter Instance
        {
            get
            {
                if (instance == null)
                {
                    Log.Error("Plotter was not initialized");
                    throw new Exception("Plotter was not initialized");
                }
                return instance;
            }
        }
        private static Plotter instance;
        public static void Init(string pythonPath)
        {
            if(instance == null)
                instance = new Plotter(pythonPath);
        }

        private string PythonPath { get; }
        private Plotter(string pythonPath)
        {
            PythonPath = pythonPath;
        }

        public byte[] GetPlot(double[] x, string[] xLabels, string title=null, double[] stds=null, int width = 12, int height=8)
        {
            File.Delete("data.json");
            File.Delete("plot.png");

            var json = JsonSerializer.Serialize(new Dictionary<string, object>
            {
                { "x", x },
                { "xLabels", xLabels },
                { "stds", stds },
                { "title", title },
                { "height", height },
                { "width", width },
            });
            File.WriteAllText("data.json", json);
            RunPlotter();
            return File.ReadAllBytes("plot.png");
        }

        private void RunPlotter()
        {
            var start = new ProcessStartInfo
            {
                FileName = PythonPath,
                Arguments = $"plot.py",
                UseShellExecute = false,
            };
            using var process = Process.Start(start);
            process.WaitForExit();
        }
    }
}
