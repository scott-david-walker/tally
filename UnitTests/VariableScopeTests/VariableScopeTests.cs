using Tally.Variables;

namespace UnitTests.VariableScopeTests;

public class VariableScopeTests
{
    [Fact]
    public void EarlierLayers_ShouldTakePriority()
    {
        var scope = new VariableScope(
            new Dictionary<string, string> { ["x"] = "high" },
            new Dictionary<string, string> { ["x"] = "low" });

        Assert.True(scope.TryGet("x", out var value));
        Assert.Equal("high", value);
    }

    [Fact]
    public void WhenVariableIsMissing_ShouldCheckLowerLayers()
    {
        var scope = new VariableScope(
            new Dictionary<string, string> { ["a"] = "1" },
            new Dictionary<string, string> { ["b"] = "2" });

        Assert.Equal("2", scope.Get("b"));
    }

    [Fact]
    public void WhenNoVariableFound_ShouldFinallyCheckEnvironment()
    {
        var name = "TESTCLI_ENV_" + Guid.NewGuid().ToString("N");
        Environment.SetEnvironmentVariable(name, "from-env");
        try
        {
            var scope = new VariableScope(new Dictionary<string, string>());
            Assert.Equal("from-env", scope.Get(name));
        }
        finally
        {
            Environment.SetEnvironmentVariable(name, null);
        }
    }

    [Fact]
    public void WhenLayerValueFound_ShouldPrioritiseItOverEnvironment()
    {
        var name = "TESTCLI_ENV_" + Guid.NewGuid().ToString("N");
        Environment.SetEnvironmentVariable(name, "from-env");
        try
        {
            var scope = new VariableScope(new Dictionary<string, string> { [name] = "from-layer" });
            Assert.Equal("from-layer", scope.Get(name));
        }
        finally
        {
            Environment.SetEnvironmentVariable(name, null);
        }
    }

    [Fact]
    public void WhenVariableHasEnvPrefix_ShouldReadFromEnv()
    {
        var name = "TESTCLI_ENV_" + Guid.NewGuid().ToString("N");
        Environment.SetEnvironmentVariable(name, "real-env");
        try
        {
            // A layer entry literally keyed "env.NAME" must NOT shadow the direct env read.
            var scope = new VariableScope(new Dictionary<string, string> { ["env." + name] = "shadow" });
            Assert.Equal("real-env", scope.Get("env." + name));
        }
        finally
        {
            Environment.SetEnvironmentVariable(name, null);
        }
    }

    [Fact]
    public void TryGet_ShouldReturnFalseForUnresolved()
    {
        var scope = new VariableScope(new Dictionary<string, string>());
        Assert.False(scope.TryGet("nope_" + Guid.NewGuid().ToString("N"), out _));
    }

    [Fact]
    public void Get_ShouldThrowForUnresolved()
    {
        var scope = new VariableScope(new Dictionary<string, string>());
        var ex = Assert.Throws<VariableResolutionException>(() => scope.Get("missingVar"));
        Assert.Equal("missingVar", ex.VariableName);
    }
}
