namespace UnitTests.Helpers;

public static class TestFileReader
{
    private const string TestDataPath = "TestFiles";
    public static string ReadFile(string path) => File.ReadAllText($"{AppContext.BaseDirectory}/{TestDataPath}/Requests/{path}");
    public static string GetFileLocation(string path) => $"{AppContext.BaseDirectory}/{TestDataPath}/Requests/{path}";
}