using Sandwych.Jasper.Tests.Models;

namespace Sandwych.Jasper.Tests;

public partial class PredOperatorsTest {

    [Fact]
    public void LessThanOperatorTest() {
        var pairs = new KeyValuePair<string, decimal>[] {
            new KeyValuePair<string, decimal>("a", 1),
            new KeyValuePair<string, decimal>("b", 2),
            new KeyValuePair<string, decimal>("c", 3),
        };

        var json = @"[ ""<"", ""Value"", 2 ]";
        var filteredByJson = pairs.AsQueryable().WhereByJson(json).ToArray();
        var filteredByLinq = pairs.Where(x => x.Value < 2).ToArray();
        Assert.Equal(filteredByLinq, filteredByJson);
    }

    [Fact]
    public void LessThanOrEqualOperatorTest() {
        var pairs = new KeyValuePair<string, decimal>[] {
            new KeyValuePair<string, decimal>("a", 1),
            new KeyValuePair<string, decimal>("b", 2),
            new KeyValuePair<string, decimal>("c", 3),
        };

        var json = @"[""<="", ""Value"", 2]";
        var filteredByJson = pairs.AsQueryable().WhereByJson(json).ToArray();
        var filteredByLinq = pairs.Where(x => x.Value <= 2).ToArray();
        Assert.Equal(filteredByLinq, filteredByJson);
    }

}