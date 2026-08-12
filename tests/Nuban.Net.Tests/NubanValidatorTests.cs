using Nuban;

namespace Nuban.Net.Tests;

public class NubanValidatorTests
{
    private const string ValidBankCode = "011";
    private const string ValidSerialNumber = "000001457";
    private const string ValidAccountNumber = "0000014579";

    public static IEnumerable<object[]> InvalidBankCodeCases()
    {
        yield return [null!, NubanValidationFailureReason.BankCodeMissing];
        yield return ["", NubanValidationFailureReason.BankCodeMissing];
        yield return ["01", NubanValidationFailureReason.BankCodeInvalidLength];
        yield return ["0111", NubanValidationFailureReason.BankCodeInvalidLength];
        yield return ["01A", NubanValidationFailureReason.BankCodeNotNumeric];
        yield return [" 11", NubanValidationFailureReason.BankCodeNotNumeric];
        yield return ["01-", NubanValidationFailureReason.BankCodeNotNumeric];
        yield return ["٤11", NubanValidationFailureReason.BankCodeNotNumeric]; // Arabic-indic digit four
    }

    public static IEnumerable<object[]> InvalidAccountNumberCases()
    {
        yield return [null!, NubanValidationFailureReason.AccountNumberMissing];
        yield return ["", NubanValidationFailureReason.AccountNumberMissing];
        yield return ["000001457", NubanValidationFailureReason.AccountNumberInvalidLength];
        yield return ["00000145799", NubanValidationFailureReason.AccountNumberInvalidLength];
        yield return ["00000145A9", NubanValidationFailureReason.AccountNumberNotNumeric];
        yield return ["000001457 ", NubanValidationFailureReason.AccountNumberNotNumeric];
        yield return ["000001-579", NubanValidationFailureReason.AccountNumberNotNumeric];
    }

    [Theory]
    [MemberData(nameof(InvalidBankCodeCases))]
    public void Validate_RejectsMalformedBankCode(string? bankCode, NubanValidationFailureReason expectedReason)
    {
        NubanValidationResult result = NubanValidator.Validate(bankCode!, ValidAccountNumber);

        Assert.False(result.IsValid);
        Assert.Equal(expectedReason, result.FailureReason);
        Assert.Null(result.SerialNumber);
        Assert.Null(result.ProvidedCheckDigit);
        Assert.Null(result.ComputedCheckDigit);
    }

    [Theory]
    [MemberData(nameof(InvalidAccountNumberCases))]
    public void Validate_RejectsMalformedAccountNumber(string? accountNumber, NubanValidationFailureReason expectedReason)
    {
        NubanValidationResult result = NubanValidator.Validate(ValidBankCode, accountNumber!);

        Assert.False(result.IsValid);
        Assert.Equal(expectedReason, result.FailureReason);
        Assert.Null(result.SerialNumber);
        Assert.Null(result.ProvidedCheckDigit);
        Assert.Null(result.ComputedCheckDigit);
    }

    [Theory]
    [MemberData(nameof(InvalidBankCodeCases))]
    public void IsValid_ReturnsFalse_ForMalformedBankCode(string? bankCode, NubanValidationFailureReason _)
    {
        Assert.False(NubanValidator.IsValid(bankCode!, ValidAccountNumber));
    }

    [Theory]
    [MemberData(nameof(InvalidAccountNumberCases))]
    public void IsValid_ReturnsFalse_ForMalformedAccountNumber(string? accountNumber, NubanValidationFailureReason _)
    {
        Assert.False(NubanValidator.IsValid(ValidBankCode, accountNumber!));
    }

    [Fact]
    public void Validate_ReportsCheckDigitMismatch_ForWellFormedButIncorrectAccountNumber()
    {
        NubanValidationResult result = NubanValidator.Validate(ValidBankCode, "0000014570");

        Assert.False(result.IsValid);
        Assert.Equal(NubanValidationFailureReason.CheckDigitMismatch, result.FailureReason);
        Assert.Equal(ValidSerialNumber, result.SerialNumber);
        Assert.Equal(0, result.ProvidedCheckDigit);
        Assert.Equal(9, result.ComputedCheckDigit);
    }

    [Fact]
    public void Validate_ReturnsValueEqualResults_ForRepeatedIdenticalInput()
    {
        NubanValidationResult first = NubanValidator.Validate(ValidBankCode, ValidAccountNumber);
        NubanValidationResult second = NubanValidator.Validate(ValidBankCode, ValidAccountNumber);

        Assert.Equal(first, second);
    }

    [Fact]
    public void Validate_PreservesLeadingZeroesInSerialNumber()
    {
        NubanValidationResult result = NubanValidator.Validate("011", "0000000220");

        Assert.True(result.IsValid);
        Assert.Equal("000000022", result.SerialNumber);
    }

    [Theory]
    [InlineData("01", "000001457")]
    [InlineData("0111", "000001457")]
    [InlineData("01A", "000001457")]
    [InlineData("", "000001457")]
    public void ComputeCheckDigit_ThrowsArgumentException_ForMalformedBankCode(string bankCode, string serialNumber)
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => NubanValidator.ComputeCheckDigit(bankCode, serialNumber));

        Assert.Equal("bankCode", exception.ParamName);
    }

    [Theory]
    [InlineData("011", "00000145")]
    [InlineData("011", "0000014579")]
    [InlineData("011", "00000145A")]
    [InlineData("011", "")]
    public void ComputeCheckDigit_ThrowsArgumentException_ForMalformedSerialNumber(string bankCode, string serialNumber)
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => NubanValidator.ComputeCheckDigit(bankCode, serialNumber));

        Assert.Equal("serialNumber", exception.ParamName);
    }

    [Fact]
    public void ComputeCheckDigit_ThrowsArgumentNullException_ForNullBankCode()
    {
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
            () => NubanValidator.ComputeCheckDigit(null!, ValidSerialNumber));

        Assert.Equal("bankCode", exception.ParamName);
    }

    [Fact]
    public void ComputeCheckDigit_ThrowsArgumentNullException_ForNullSerialNumber()
    {
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
            () => NubanValidator.ComputeCheckDigit(ValidBankCode, null!));

        Assert.Equal("serialNumber", exception.ParamName);
    }

    [Fact]
    public void NubanFormat_ExposesDocumentedLengths()
    {
        Assert.Equal(3, NubanFormat.BankCodeLength);
        Assert.Equal(9, NubanFormat.SerialNumberLength);
        Assert.Equal(1, NubanFormat.CheckDigitLength);
        Assert.Equal(10, NubanFormat.AccountNumberLength);
    }
}
