using Sandwych.Jasper.Tests.Models;

namespace Sandwych.Jasper.Tests;

public partial class PredOperatorsTest {

    [Fact]
    public void AndOperatorTest1() {
        var pairs = new KeyValuePair<string, decimal>[] {
            new KeyValuePair<string, decimal>("a", 1),
            new KeyValuePair<string, decimal>("b", 2),
            new KeyValuePair<string, decimal>("c", 3),
        };

        var json = """
        [
            "and", 
            ["=", "Key", "b"],
            ["=", "Value", 2]
        ]
        """;
        var filteredByJson = pairs.AsQueryable().WhereByJson(json).ToArray();
        var filteredByLinq = pairs.Where(x => x.Key == "b" && x.Value == 2).ToArray();
        Assert.Equal(filteredByLinq, filteredByJson);
    }

    [Fact]
    public void OrOperatorTest1() {
        var pairs = new KeyValuePair<string, decimal>[] {
            new KeyValuePair<string, decimal>("a", 1),
            new KeyValuePair<string, decimal>("b", 2),
            new KeyValuePair<string, decimal>("c", 3),
        };

        var json = """
        [
            "or", 
            ["=", "Key", "b"],
            ["=", "Value", 1]
        ]
        """;
        var filteredByJson = pairs.AsQueryable().WhereByJson(json).ToArray();
        var filteredByLinq = pairs.Where(x => x.Key == "b" || x.Value == 1).ToArray();
        Assert.Equal(filteredByLinq, filteredByJson);
    }

    [Fact]
    public void NotOperatorTest1() {
        var pairs = new KeyValuePair<string, decimal>[] {
            new KeyValuePair<string, decimal>("a", 1),
            new KeyValuePair<string, decimal>("b", 2),
            new KeyValuePair<string, decimal>("c", 3),
        };

        var json = """
        [
            "and", 
            ["=", "Key", "b"],
            ["not", ["=", "Value", 1]]
        ]
        """;
        var filteredByJson = pairs.AsQueryable().WhereByJson(json).ToArray();
        var filteredByLinq = pairs.Where(x => x.Key == "b" && !(x.Value == 1)).ToArray();
        Assert.Equal(filteredByLinq, filteredByJson);
    }


}