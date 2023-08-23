/*using System.Text.Json;
using EngineeringSymbols.Tools.SvgParser;
using EngineeringSymbols.Tools.SvgParser.Models;
using EngineeringSymbols.Tools.Validation;
using LanguageExt.Pretty;
using Xunit.Abstractions;

namespace EngineeringSymbols.Test;

public class TurtleParserTest
{
    public class SvgParserTests
    {
        ITestOutputHelper output;

        public SvgParserTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        private void OutputAsJson(object obj)
        {
            output.WriteLine(JsonSerializer.Serialize(obj, new JsonSerializerOptions {WriteIndented = true}));
        }

        [Fact]
        public void Test1()
        {
            var path = Path.GetFullPath("./SvgTestFiles/Symbol1.ttl");

            var turtle = File.ReadAllText(path);

            var result = RdfParser.TurtleToEngineeringSymbol(turtle);
            
            result.IfFail(seq =>
            {
                var errors = seq.ToList();
                output.WriteLine($"{errors.Count} parse errors:");
                errors.ForEach(error => output.WriteLine(error.Value));
            });
            
            Assert.True(result.IsSuccess);
        }
        
        [Fact]
        public void FailsOnInvalidFile()
        {
            var path = Path.GetFullPath("./SvgTestFiles/Symbol1_error.ttl");

            var turtle = File.ReadAllText(path);

            var result = RdfParser.TurtleToEngineeringSymbol(turtle);
            
            result.IfFail(seq =>
            {
                var errors = seq.ToList();
                output.WriteLine($"{errors.Count} parse errors:");
                errors.ForEach(error => output.WriteLine(error.Value));
            });
            
            Assert.True(result.IsFail);
        }
    }
}*/