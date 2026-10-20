using Tally.Variables;

namespace UnitTests.TemplateEngineTests;

public class TemplateEngineTests
{
    [Fact]
    public void CanRenderMultipleTokensAndTolerateWhitespace()
    {
        var scope = new VariableScope(new Dictionary<string, string>
        {
            ["baseUrl"] = "https://api.test",
            ["id"] = "42",
        });

        var result = TemplateEngine.Render("{{baseUrl}}/orders/{{ id }}", scope);
        Assert.Equal("https://api.test/orders/42", result);
    }

    [Fact]
    public void WhenVariableIsMissing_ShouldThrow()
    {
        var scope = new VariableScope(new Dictionary<string, string>());
        Assert.Throws<VariableResolutionException>(() => TemplateEngine.Render("{{missing}}", scope));
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("hello", "hello")]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    [InlineData(42, "42")]
    public void Stringify_ShouldProduceExpectedText(object? input, string expected)
        => Assert.Equal(expected, TemplateEngine.Stringify(input));

    [Fact]
    public void Stringify_ShouldUseInvariantForDoubles()
        => Assert.Equal("1.5", TemplateEngine.Stringify(1.5));

    [Fact]
    public void ToStringLayer_ShouldStringifyAllValues()
    {
        var layer = TemplateEngine.ToStringLayer(new Dictionary<string, object?>
        {
            ["a"] = 1,
            ["b"] = "text",
            ["c"] = true,
        });

        Assert.Equal("1", layer["a"]);
        Assert.Equal("text", layer["b"]);
        Assert.Equal("true", layer["c"]);
    }
}
