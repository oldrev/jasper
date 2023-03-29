using Sandwych.Jasper.Tests.Models;

namespace Sandwych.Jasper.Tests;

public partial class PredOperatorsTest {

    [Fact]
    public void InOperatorTest1() {
        var pairs = new KeyValuePair<string, decimal>[] {
            new KeyValuePair<string, decimal>("a", 1),
            new KeyValuePair<string, decimal>("b", 2),
            new KeyValuePair<string, decimal>("c", 3),
        };

        var json = """
            [
                "and", 
                ["in", "Value", [1, 2]], 
                ["=", "Key", "b"]
            ]
        """;
        var filteredByJson = pairs.AsQueryable().WhereByJson(json).ToArray();
        var filteredByLinq = pairs.Where(x => (x.Value == 1 || x.Value == 2) && x.Key == "b").ToArray();
        Assert.Equal(filteredByLinq, filteredByJson);
    }

    [Fact]
    public void InOperatorTest2() {
        var pairs = new KeyValuePair<string, decimal>[] {
            new KeyValuePair<string, decimal>("a", 1),
            new KeyValuePair<string, decimal>("b", 2),
            new KeyValuePair<string, decimal>("c", 3),
        };

        var json = """["in", "Value", [1, 2]]""";
        var filteredByJson = pairs.AsQueryable().WhereByJson(json).ToArray();
        var filteredByLinq = pairs.Where(x => (x.Value == 1 || x.Value == 2)).ToArray();
        Assert.Equal(filteredByLinq, filteredByJson);
    }
}