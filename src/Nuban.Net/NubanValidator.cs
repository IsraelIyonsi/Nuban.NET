namespace Nuban;

/// <summary>
/// Validates Nigerian NUBAN (Nigeria Uniform Bank Account Number) account numbers using the
/// Central Bank of Nigeria (CBN) modulus-10 check digit algorithm.
/// </summary>
/// <remarks>
/// <para>
/// A NUBAN account number is 10 digits: a 9-digit account serial number followed by a single
/// check digit. The check digit is computed from the 3-digit CBN bank code for the receiving
/// bank concatenated with the 9-digit serial number, applying the repeating weight pattern
/// 3, 7, 3 across all 12 digits, summing the weighted digits, reducing the sum modulo 10, and
/// subtracting the remainder from 10. A result of 10 maps to a check digit of 0.
/// </para>
/// <para>
/// This library does not ship a registry of CBN bank codes. Callers supply the bank code for
/// the account being validated, typically selected by the end user from their own bank list or
/// supplied by a payment provider.
/// </para>
/// </remarks>
public static class NubanValidator
{
    private const int Modulus = 10;
    private static readonly int[] WeightCycle = [3, 7, 3];

    /// <summary>
    /// Computes the CBN check digit for a bank code and account serial number.
    /// </summary>
    /// <param name="bankCode">
    /// The 3-digit CBN bank code for the receiving bank, as a string of ASCII digits with any
    /// leading zeros preserved.
    /// </param>
    /// <param name="serialNumber">
    /// The 9-digit account serial number, as a string of ASCII digits with any leading zeros
    /// preserved.
    /// </param>
    /// <returns>The check digit, in the range 0 to 9.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="bankCode"/> or <paramref name="serialNumber"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="bankCode"/> is not exactly <see cref="NubanFormat.BankCodeLength"/> ASCII
    /// digits, or <paramref name="serialNumber"/> is not exactly
    /// <see cref="NubanFormat.SerialNumberLength"/> ASCII digits.
    /// </exception>
    public static int ComputeCheckDigit(string bankCode, string serialNumber)
    {
        ArgumentNullException.ThrowIfNull(bankCode);
        ArgumentNullException.ThrowIfNull(serialNumber);

        if (!IsAsciiDigitsOfLength(bankCode, NubanFormat.BankCodeLength))
        {
            throw new ArgumentException(
                $"Bank code must be exactly {NubanFormat.BankCodeLength} ASCII digits.",
                nameof(bankCode));
        }

        if (!IsAsciiDigitsOfLength(serialNumber, NubanFormat.SerialNumberLength))
        {
            throw new ArgumentException(
                $"Serial number must be exactly {NubanFormat.SerialNumberLength} ASCII digits.",
                nameof(serialNumber));
        }

        return ComputeCheckDigitUnchecked(bankCode, serialNumber);
    }

    /// <summary>
    /// Reports whether a NUBAN account number is well-formed and its check digit matches the
    /// one computed for the given bank code.
    /// </summary>
    /// <param name="bankCode">The 3-digit CBN bank code for the receiving bank.</param>
    /// <param name="accountNumber">The 10-digit NUBAN account number to validate.</param>
    /// <returns>
    /// <see langword="true"/> when both inputs are well-formed and the check digit matches;
    /// otherwise <see langword="false"/>. Call <see cref="Validate(string, string)"/> to find
    /// out why a <see langword="false"/> result occurred.
    /// </returns>
    public static bool IsValid(string bankCode, string accountNumber) =>
        Validate(bankCode, accountNumber).IsValid;

    /// <summary>
    /// Validates a NUBAN account number against its check digit for the given bank code, and
    /// reports the computed and provided check digits alongside the outcome.
    /// </summary>
    /// <param name="bankCode">The 3-digit CBN bank code for the receiving bank.</param>
    /// <param name="accountNumber">The 10-digit NUBAN account number to validate.</param>
    /// <returns>A <see cref="NubanValidationResult"/> describing the outcome.</returns>
    public static NubanValidationResult Validate(string bankCode, string accountNumber)
    {
        if (string.IsNullOrEmpty(bankCode))
        {
            return Failure(NubanValidationFailureReason.BankCodeMissing);
        }

        if (!IsAsciiDigitsOfLength(bankCode, NubanFormat.BankCodeLength))
        {
            return Failure(bankCode.Length != NubanFormat.BankCodeLength
                ? NubanValidationFailureReason.BankCodeInvalidLength
                : NubanValidationFailureReason.BankCodeNotNumeric);
        }

        if (string.IsNullOrEmpty(accountNumber))
        {
            return Failure(NubanValidationFailureReason.AccountNumberMissing);
        }

        if (!IsAsciiDigitsOfLength(accountNumber, NubanFormat.AccountNumberLength))
        {
            return Failure(accountNumber.Length != NubanFormat.AccountNumberLength
                ? NubanValidationFailureReason.AccountNumberInvalidLength
                : NubanValidationFailureReason.AccountNumberNotNumeric);
        }

        string serialNumber = accountNumber[..NubanFormat.SerialNumberLength];
        int providedCheckDigit = accountNumber[NubanFormat.SerialNumberLength] - '0';
        int computedCheckDigit = ComputeCheckDigitUnchecked(bankCode, serialNumber);

        return new NubanValidationResult(
            IsValid: providedCheckDigit == computedCheckDigit,
            serialNumber,
            providedCheckDigit,
            computedCheckDigit,
            providedCheckDigit == computedCheckDigit
                ? NubanValidationFailureReason.None
                : NubanValidationFailureReason.CheckDigitMismatch);
    }

    private static int ComputeCheckDigitUnchecked(string bankCode, string serialNumber)
    {
        string combinedDigits = string.Concat(bankCode, serialNumber);
        int weightedSum = 0;

        for (int index = 0; index < combinedDigits.Length; index++)
        {
            int digit = combinedDigits[index] - '0';
            int weight = WeightCycle[index % WeightCycle.Length];
            weightedSum += digit * weight;
        }

        int remainder = weightedSum % Modulus;
        int checkDigit = Modulus - remainder;
        return checkDigit == Modulus ? 0 : checkDigit;
    }

    private static bool IsAsciiDigitsOfLength(string value, int expectedLength)
    {
        if (value.Length != expectedLength)
        {
            return false;
        }

        foreach (char character in value)
        {
            if (character is < '0' or > '9')
            {
                return false;
            }
        }

        return true;
    }

    private static NubanValidationResult Failure(NubanValidationFailureReason reason) =>
        new(IsValid: false, SerialNumber: null, ProvidedCheckDigit: null, ComputedCheckDigit: null, reason);
}
