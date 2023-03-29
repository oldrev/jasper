using Sandwych.Jasper.Tests.Models;

namespace Sandwych.Jasper.Tests;

public partial class PredOperatorsTest {

    [Fact]
    public void NavigationPropertyShouldBeOk() {
        var pairs = new KeyValuePair<string, decimal>[] {
            new KeyValuePair<string, decimal>("a", 1),
            new KeyValuePair<string, decimal>("bb", 2),
            new KeyValuePair<string, decimal>("ccc", 3),
        };

        var json = """["=", "Key.Length", 2]""";
        var filteredByJson = pairs.AsQueryable().WhereByJson(json).ToArray();
        var filteredByLinq = pairs.Where(x => x.Key.Length == 2).ToArray();
        Assert.Equal(filteredByLinq, filteredByJson);
    }


}