using System;
using System.IO;
using System.Linq;

namespace ConsoleApp1
{
    /// Using Maybe monad for splitting result to data OR error
    class Program
    {
        static void Main(string[] args)
        {
            var fileName = Path.Combine(Environment.GetEnvironmentVariable("SystemRoot"), "win.ini");

            var result = parseFile(fileName);
            switch (result) // readable checking of result (+)
            {
                case Success<ParseResult> success:
                    Console.WriteLine("Success parsed, {0} sections, {1} lines", success.Value.SectionCount, success.Value.Lines.Length);
                    break;
                
                case Failed<ParseResult> failed:
                    Console.WriteLine("Failed to parse: {0}", failed.Message);
                    break;
            }
        }
        
        // returning of Maybe<T> indicate only two state of method result - valid model OR error (+)
        static Maybe<ParseResult> parseFile(string fileName)
        {
            if (!File.Exists(fileName))
                return Maybe<ParseResult>.Failed("File not found"); // readable Failed result (+)
            
            // also easy to find in sources by ".Failed" or "<T>.Failed" keywords for finding error results
            // (and no ErrorMessage/Error/Fail and some other property names indicated error (+)

            try
            {
                var lines = File.ReadAllLines(fileName);

                // implicit cast to Success<T> (+)
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
                return Maybe<ParseResult>.Failed(e.Message); // "not enough rights" or parsing error or something wrong
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