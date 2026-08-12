namespace Nuban;

/// <summary>
/// The structured outcome of <see cref="NubanValidator.Validate(string, string)"/>.
/// </summary>
/// <param name="IsValid">
/// <see langword="true"/> when the account number is well-formed and its check digit matches
/// the one computed from the bank code and serial number.
/// </param>
/// <param name="SerialNumber">
/// The 9-digit account serial number extracted from the account number, or <see langword="null"/>
/// when the account number was not well-formed enough to extract a serial number from.
/// </param>
/// <param name="ProvidedCheckDigit">
/// The check digit found in the last position of the account number, or <see langword="null"/>
/// when the account number was not well-formed enough to extract a check digit from.
/// </param>
/// <param name="ComputedCheckDigit">
/// The check digit computed from the bank code and serial number using the CBN algorithm, or
/// <see langword="null"/> when either input was too malformed to run the algorithm on.
/// </param>
/// <param name="FailureReason">
/// <see cref="NubanValidationFailureReason.None"/> when <paramref name="IsValid"/> is
/// <see langword="true"/>; otherwise the specific reason validation failed.
/// </param>
public readonly record struct NubanValidationResult(
    bool IsValid,
    string? SerialNumber,
    int? ProvidedCheckDigit,
    int? ComputedCheckDigit,
    NubanValidationFailureReason FailureReason);
