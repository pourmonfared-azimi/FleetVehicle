using System.Runtime.CompilerServices;
using Xunit;

namespace FleetGraph.Testing;

/// <summary>
/// Schema snapshot assertion: the schema must not change by accident. To accept an intended
/// change, run the tests with <c>UPDATE_SNAPSHOTS=1</c> and commit the updated file.
/// </summary>
public static class SchemaSnapshot
{
    public static void Match(
        string actualSdl,
        string snapshotName,
        [CallerFilePath] string callerFilePath = "")
    {
        var directory = Path.Combine(Path.GetDirectoryName(callerFilePath)!, "__snapshots__");
        var snapshotPath = Path.Combine(directory, snapshotName);
        var actual = Normalize(actualSdl);
        var update = Environment.GetEnvironmentVariable("UPDATE_SNAPSHOTS") == "1";
        var isCi = string.Equals(Environment.GetEnvironmentVariable("CI"), "true", StringComparison.OrdinalIgnoreCase);

        if (!File.Exists(snapshotPath) || update)
        {
            if (isCi && !update)
            {
                Assert.Fail($"Snapshot '{snapshotPath}' is missing. Run the tests locally and commit it.");
            }

            Directory.CreateDirectory(directory);
            File.WriteAllText(snapshotPath, actual);
            return;
        }

        var expected = Normalize(File.ReadAllText(snapshotPath));

        if (!string.Equals(expected, actual, StringComparison.Ordinal))
        {
            File.WriteAllText(snapshotPath + ".received", actual);
            Assert.Fail(
                $"The schema differs from the snapshot '{snapshotName}'. " +
                $"Compare with '{snapshotPath}.received'. If the change is intended, " +
                "re-run with UPDATE_SNAPSHOTS=1 and check docs/adr/0003 for breaking changes.");
        }
    }

    private static string Normalize(string text) => text.Replace("\r\n", "\n").Trim() + "\n";
}
