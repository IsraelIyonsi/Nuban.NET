using System.Linq;

namespace Nuban.Net.Tests;

/// <summary>
/// Reads the CSV worked-example fixtures shipped under <c>fixtures/</c> into xUnit
/// <see cref="MemberDataAttribute"/> rows.
/// </summary>
internal static class FixtureLoader
{
    private const string FixturesDirectoryName = "fixtures";
    private const char FieldSeparator = ',';
    private const int HeaderRowCount = 1;
    private const int MaxFieldCount = 4;

    public static IEnumerable<object[]> LoadCheckDigitCases(string fileName)
    {
        string path = Path.Combine(AppContext.BaseDirectory, FixturesDirectoryName, fileName);

        foreach (string line in File.ReadLines(path).Skip(HeaderRowCount))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            string[] fields = line.Split(FieldSeparator, MaxFieldCount);
            string bankCode = fields[0];
            string serialNumber = fields[1];
            int checkDigit = int.Parse(fields[2]);

            yield return [bankCode, serialNumber, checkDigit];
        }
    }
}
