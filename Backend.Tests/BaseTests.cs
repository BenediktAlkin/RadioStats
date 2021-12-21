using NUnit.Framework;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Tests
{
    public class BaseTests
    {
        [SetUp]
        public virtual void SetUp()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("tests.log")
                .CreateLogger();
        }
        [TearDown]
        public virtual void TearDown()
        {
            Log.CloseAndFlush();
        }
    }
}
