using System;
using System.IO;
using System.Linq;

namespace Initial
{
    // Easy example for demonstrate advantages of using monads for splitting model and error
    // This example written as commonly used 
    class Program
    {
        static void Main(string[] args)
        {
            var fileName = Path.Combine(Environment.GetEnvironmentVariable("SystemRoot"), "win.ini");

            var result = parseFile(fileName);
            if (result.ErrorMessage == null) // need to explore "what if (-) 
            {
                Console.WriteLine("Success parsed, {0} sections, {1} lines", result.SectionCount, result.Lines.Length);
            }
            else // need to explore code - 'else' of what? (-)
            {
                Console.WriteLine("Failed to parse: {0}", result.ErrorMessage);
            }
        }

        static ParseResult parseFile(string fileName)
        {
            if (!File.Exists(fileName))
                return new ParseResult() {ErrorMessage = "File not found"}; // rest of model has default values/invalid state (-)

            try
            {
                var lines = File.ReadAllLines(fileName);

                return new ParseResult()
                {
                    // ErrorMessage==null indicate success result and if model have valid state - ErrorMessage is redundant (-) 
                    FileLastModified = new FileInfo(fileName).LastWriteTimeUtc,
                    Lines = lines,
                    SectionCount = lines.Count(p => p.StartsWith("["))
                };
            }
            catch (Exception e)
            {
                return new ParseResult() {ErrorMessage = e.Message}; // "not enough rights" or parsing error or something wrong
            }
        }
    }

    // model contain mutually exclusive error AND data (thick -)
    class ParseResult
    {
        public string ErrorMessage { get; set; }

        public DateTime FileLastModified { get; set; }
        public string[] Lines { get; set; }
        public int SectionCount { get; set; }
    }
}