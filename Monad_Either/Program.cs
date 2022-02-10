using System;
using System.IO;
using System.Linq;
using ConsoleApp1;

namespace Monad_Either
{
    // Example of using Either<L,R> monad for returing one of two states
    class Program
    {
        static void Main(string[] args)
        {
            var fileName = Path.Combine(Environment.GetEnvironmentVariable("SystemRoot"), "win.ini");

            var result = parseFile(fileName);
            
            result.Case(
                succ => Console.WriteLine("Success parsed, {0} sections, {1} lines", succ.SectionCount, succ.Lines.Length),
                fail => Console.WriteLine("Failed to parse: [{0}] {1}", fail.Error, fail.Error));
            
            if(result.TryGetValue(out ParseResult parseResult))
            {
                // do something for success model
            }

            // get value or pass some DEFAULT value
            var value1 = result.GetValueOrDefault(new ParseResult() {Lines = Array.Empty<string>(), SectionCount = 0, FileLastModified = DateTime.Now});
            
            // get value or call factory for create some DEFAULT value
            var value2 = result.GetValueOrDefault(() => new ParseResult() {Lines = Array.Empty<string>(), SectionCount = 0, FileLastModified = DateTime.Now});
        }

        // returning of Either<L,R> indicate only two states of method result (+)
        // zero allocations in Heap (+)
        static Either<ParseResult, (string Error, int Code)> parseFile(string fileName)
        {
            if (!File.Exists(fileName))
                return ("File not found", 0); // implicit cast to second state

            try
            {
                var lines = File.ReadAllLines(fileName);

                // implicit cast to first state (+)
                // only valid data passed to model and no other properties
                return new ParseResult()
                {
                    FileLastModified = new FileInfo(fileName).LastWriteTimeUtc,
                    Lines = lines,
                    SectionCount = lines.Count(p => p.StartsWith("["))
                };
            }
            catch (Exception e)
            {
                return (e.Message, 1);
            }
        }
    }
    
    // model contain only valid data and no other properties (+)
    class ParseResult
    {
        public DateTime FileLastModified { get; set; }
        public string[] Lines { get; set; }
        public int SectionCount { get; set; }
    }
}