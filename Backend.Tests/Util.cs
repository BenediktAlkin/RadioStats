using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Backend.Tests
{
    public static class Util
    {
        public static TReturn InvokePrivateMethod<TReturn>(Type classType, string methodName, params object[] parameters)
        {
            var method = classType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
            var result = method.Invoke(null, parameters);
            return (TReturn)result;
        }

        public static string RemoveWhitespaceOutsideQuotes(string input) => 
            Regex.Replace(input, @"(""[^""\\]*(?:\\.[^""\\]*)*"")|\s+", "$1");
    }
}
