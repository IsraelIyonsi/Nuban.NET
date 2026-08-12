using Nuban;

namespace Nuban.Net.Tests;

/// <summary>
/// Anchors <see cref="NubanValidator"/> against embedded worked examples.
/// </summary>
/// <remarks>
/// <c>official-worked-examples.csv</c> holds the two worked examples published in the CBN's
/// NUBAN check digit specification (bank code 011, First Bank): serial 000001457 with check
/// digit 9, and serial 000000022 with check digit 0. Both were independently recomputed by
/// hand against the documented algorithm (weight pattern 3, 7, 3 across the 3-digit bank code
/// plus 9-digit serial, summed, reduced modulo 10, subtracted from 10, with a result of 10
/// mapped to 0) before being embedded here, and both match exactly.
///
/// <c>self-generated-examples.csv</c> holds additional cases computed with the same, now
/// CBN-verified, algorithm to broaden coverage (real Nigerian bank codes, both check-digit
/// branches, and every possible check digit 0 through 9). These are algorithm-consistent, not
/// independently published examples.
/// </remarks>
public class WorkedExampleFixtureTests
{
    public static IEnumerable<object[]> OfficialCases =>
        FixtureLoader.LoadCheckDigitCases("official-worked-examples.csv");

    public static IEnumerable<object[]> SelfGeneratedCases =>
        FixtureLoader.LoadCheckDigitCases("self-generated-examples.csv");

    [Theory]
    [MemberData(nameof(OfficialCases))]
    public void ComputeCheckDigit_MatchesCbnPublishedWorkedExample(
        string bankCode, string serialNumber, int expectedCheckDigit)
    {
        int actual = NubanValidator.ComputeCheckDigit(bankCode, serialNumber);

        Assert.Equal(expectedCheckDigit, actual);
    }

    [Theory]
    [MemberData(nameof(OfficialCases))]
    public void Validate_AcceptsCbnPublishedWorkedExample(
        string bankCode, string serialNumber, int expectedCheckDigit)
    {
        string accountNumber = serialNumber + expectedCheckDigit;

        NubanValidationResult result = NubanValidator.Validate(bankCode, accountNumber);

        Assert.True(result.IsValid);
        Assert.Equal(NubanValidationFailureReason.None, result.FailureReason);
        Assert.Equal(serialNumber, result.SerialNumber);
        Assert.Equal(expectedCheckDigit, result.ProvidedCheckDigit);
        Assert.Equal(expectedCheckDigit, result.ComputedCheckDigit);
    }

    [Theory]
    [MemberData(nameof(SelfGeneratedCases))]
    public void ComputeCheckDigit_MatchesSelfGeneratedAlgorithmConsistentExample(
        string bankCode, string serialNumber, int expectedCheckDigit)
    {
        int actual = NubanValidator.ComputeCheckDigit(bankCode, serialNumber);

        Assert.Equal(expectedCheckDigit, actual);
    }

    [Theory]
    [MemberData(nameof(SelfGeneratedCases))]
    public void IsValid_AcceptsSelfGeneratedAlgorithmConsistentExample(
        string bankCode, string serialNumber, int expectedCheckDigit)
    {
        string accountNumber = serialNumber + expectedCheckDigit;

        Assert.True(NubanValidator.IsValid(bankCode, accountNumber));
    }

    [Theory]
    [MemberData(nameof(OfficialCases))]
    [MemberData(nameof(SelfGeneratedCases))]
    public void Validate_RejectsWorkedExampleWithCorruptedCheckDigit(
        string bankCode, string serialNumber, int expectedCheckDigit)
    {
        int corruptedCheckDigit = (expectedCheckDigit + 1) % 10;
        string accountNumber = serialNumber + corruptedCheckDigit;

        NubanValidationResult result = NubanValidator.Validate(bankCode, accountNumber);

        Assert.False(result.IsValid);
        Assert.Equal(NubanValidationFailureReason.CheckDigitMismatch, result.FailureReason);
        Assert.Equal(corruptedCheckDigit, result.ProvidedCheckDigit);
        Assert.Equal(expectedCheckDigit, result.ComputedCheckDigit);
    }

    [Theory]
    [MemberData(nameof(OfficialCases))]
    [MemberData(nameof(SelfGeneratedCases))]
    public void Validate_RejectsWorkedExampleWithCorruptedLeadingSerialDigit(
        string bankCode, string serialNumber, int expectedCheckDigit)
    {
        char originalDigit = serialNumber[0];
        char corruptedDigit = (char)('0' + ((originalDigit - '0' + 1) % 10));
        string corruptedSerial = corruptedDigit + serialNumber[1..];
        string accountNumber = corruptedSerial + expectedCheckDigit;

        NubanValidationResult result = NubanValidator.Validate(bankCode, accountNumber);

        Assert.False(result.IsValid);
        Assert.Equal(NubanValidationFailureReason.CheckDigitMismatch, result.FailureReason);
    }

    [Theory]
    [MemberData(nameof(OfficialCases))]
    [MemberData(nameof(SelfGeneratedCases))]
    public void ComputeCheckDigit_AlwaysReturnsASingleDigit(
        string bankCode, string serialNumber, int expectedCheckDigit)
    {
        int actual = NubanValidator.ComputeCheckDigit(bankCode, serialNumber);

        Assert.InRange(actual, 0, 9);
        Assert.Equal(expectedCheckDigit, actual);
    }
}
