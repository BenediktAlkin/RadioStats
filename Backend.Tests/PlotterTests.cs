using Backend.Entities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Tests
{
    public class PlotterTests : BaseTests
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            Plotter.Init(@"C:\Users\bened\AppData\Local\Programs\Python\Python310\python.exe");
        }

        [Test]
        public void Plotter_ExamplePlot()
        {
            var x = new double[]
            { 
                1.0, 0.2, 0.4, 
                0.3, 1.0, 0.6, 
                0.9, 0.8, 0.2, 
                0.75, 0.3, 0.25, 
                1.0, 0.2, 0.4, 
                0.3, 1.0, 0.6, 
                0.9, 0.8, 0.2, 
                0.75, 0.3, 0.25,
            };
            var labels = Enumerable.Range(0, x.Length).Select(i => $"{i:D2}:00").ToArray();
            var plot = Plotter.Instance.GetPlot(x, labels);
            var expected = File.ReadAllBytes("Resources/example_plot.png");
            Assert.AreEqual(expected, plot);
        }
    }
}
