namespace Nuban;

/// <summary>
/// Structural constants for the Central Bank of Nigeria (CBN) 10-digit NUBAN
/// (Nigeria Uniform Bank Account Number) format.
/// </summary>
public static class NubanFormat
{
    /// <summary>
    /// The number of digits in a CBN bank code (for example <c>"011"</c> for First Bank).
    /// Bank codes are assigned by the CBN and are not shipped with this library; callers
    /// supply the bank code for the account they are validating.
    /// </summary>
    public const int BankCodeLength = 3;

    /// <summary>
    /// The number of digits in the account serial number, excluding the trailing check digit.
    /// </summary>
    public const int SerialNumberLength = 9;

    /// <summary>
    /// The number of digits in the check digit component of a NUBAN account number.
    /// </summary>
    public const int CheckDigitLength = 1;

    /// <summary>
    /// The total number of digits in a NUBAN account number, equal to
    /// <see cref="SerialNumberLength"/> plus <see cref="CheckDigitLength"/>.
    /// </summary>
    public const int AccountNumberLength = SerialNumberLength + CheckDigitLength;
}
