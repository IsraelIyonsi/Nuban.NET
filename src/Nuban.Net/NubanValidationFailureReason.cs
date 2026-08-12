namespace Nuban;

/// <summary>
/// Explains why <see cref="NubanValidator.Validate(string, string)"/> rejected an input,
/// or indicates that validation succeeded.
/// </summary>
public enum NubanValidationFailureReason
{
    /// <summary>Validation succeeded; the check digit matches.</summary>
    None = 0,

    /// <summary>The bank code was null or an empty string.</summary>
    BankCodeMissing,

    /// <summary>The bank code was not exactly <see cref="NubanFormat.BankCodeLength"/> characters long.</summary>
    BankCodeInvalidLength,

    /// <summary>The bank code contained a character that is not an ASCII digit.</summary>
    BankCodeNotNumeric,

    /// <summary>The account number was null or an empty string.</summary>
    AccountNumberMissing,

    /// <summary>
    /// The account number was not exactly <see cref="NubanFormat.AccountNumberLength"/> characters long.
    /// </summary>
    AccountNumberInvalidLength,

    /// <summary>The account number contained a character that is not an ASCII digit.</summary>
    AccountNumberNotNumeric,

    /// <summary>
    /// Both inputs were well-formed, but the check digit computed from the bank code and
    /// account serial number does not match the check digit supplied in the account number.
    /// </summary>
    CheckDigitMismatch,
}
