using EngineeringSymbols.Tools.SvgParser;
using LanguageExt.Pretty;
using Xunit.Abstractions;
using System.Text.Json;
using EngineeringSymbols.Tools.Models;
using EngineeringSymbols.Tools.SvgParser.Models;

namespace EngineeringSymbols.Test;


public class SvgParserTests
{
    ITestOutputHelper output;

    public SvgParserTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    private void OutputAsJson(object obj)
    {
        output.WriteLine(JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true }));
    }
    
    [Fact]
    public void AFileThatNotExistsDontParse()
    {
        var result = SvgParser.FromFile(TestFilePaths.AFileThatNotExists);
        
        Assert.False(result.IsSuccess);

        Assert.True(result.Match(
            _ => false,
            failure =>
        {
            Assert.True(failure is SvgParseErrorException);
            output.WriteLine($"Exception message: {failure.Message}");
            return true;
        }));
    }
    
    [Fact]
    public void InvalidFileContentDontParse()
    {
        var result = SvgParser.FromFile(TestFilePaths.InvalidRootElement);
        
        // Exception should be thrown
        Assert.False(result.IsSuccess);

        Assert.True(result.Match(
            _ => false,
            failure =>
            {
                Assert.True(failure is SvgParseErrorException);
                output.WriteLine($"Exception message: {failure.Message}");
                return true;
            }));
    }
    
    [Fact]
    public void InvalidViewBoxDontParse()
    {
        var result = SvgParser.FromFile(TestFilePaths.InvalidViewBox);
        
        Assert.True(result.IsSuccess);

        Assert.False(result.Match(
            res =>
            {
                output.WriteLine("Parse Errors:");
                OutputAsJson(res.ParseErrors);
                return res.IsSuccess;
            },
            failure =>
            {
                Assert.True(failure is SvgParseErrorException);
                output.WriteLine($"Exception message: {failure.Message}");
                return true;
            }));
    }
    
    [Theory]
    [InlineData(TestFilePaths.File1)]
    [InlineData(TestFilePaths.File2)]
    public void ValidFilesParses(string filePath)
    {
        var fileParses = SvgParser.FromFile(filePath).Match(result =>
        {
            var symbol = result.EngineeringSymbol;
            Assert.NotNull(symbol);
            if (result.IsSuccess)
            {
                AssertValidEngineeringSymbol(symbol);
            }
            OutputAsJson(result.ParseErrors);
            OutputAsJson(symbol);
            OutputAsJson(result.IsSuccess);
            return result.IsSuccess;
        }, failure =>
        {
            output.WriteLine($"Unexpected exception: {failure.Message}");
            return false;
        });
        
        Assert.True(fileParses);
    }

    [Fact]
    public void InvalidConnectors()
    {
        Assert.False(SvgParser
            .FromFile(TestFilePaths.InvalidConnectors)
            .Match(result =>
            {
                // Invalid connectors does not throw, but is part of the ParseError list
                output.WriteLine("Parse Errors:");
                OutputAsJson(result.ParseErrors);
                output.WriteLine("Connectors:");
                OutputAsJson(result.EngineeringSymbol.Connectors);
                return result.IsSuccess;
            },
            failure =>
            {
                output.WriteLine($"Exception message: {failure.Message}");
                // A connector parse error should not throw exception
                return true;
            }));
    }
    
    public static void AssertValidEngineeringSymbol(EngineeringSymbol symbol)
    {
        //Assert.NotNull(symbol.Id);
        Assert.True(symbol.Height % 24 == 0);
        Assert.True(symbol.Width % 24 == 0);
        // Both strings cant be null at the same time
        Assert.True(symbol.GeometryString != null && symbol.SvgString != null);
        
        Assert.NotNull(symbol.Connectors);
        
        foreach (var c in symbol.Connectors)
        {
            Assert.NotNull(c.Id);
            Assert.True(c.Id.Length > 0);
            
            Assert.NotNull(c.RelativePosition);
            Assert.NotNull(c.RelativePosition.X);
            Assert.NotNull(c.RelativePosition.Y);
            
            Assert.True(c.RelativePosition.X >= 0);
            Assert.True(c.RelativePosition.Y >= 0);

            Assert.NotNull(c.Direction);
            Assert.InRange(c.Direction, 0, 360);
        }
    }
}