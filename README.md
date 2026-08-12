# Nuban.NET

Validate Nigerian NUBAN bank account numbers in .NET. Central Bank of Nigeria (CBN) check-digit
verification and structure checks for the 10-digit NUBAN format. Zero external dependencies.

Every Nigerian bank account number carries a check digit that lets you catch a mistyped or
corrupted account number before you send money to the wrong place. NUBAN validation shows up
constantly in Nigerian fintech: payout forms, KYC onboarding, bank transfer confirmation screens.
On NuGet there has been nothing clean and dependency-free for it: you either hand-roll the CBN
modulus-10 algorithm yourself (easy to get subtly wrong on leading zeros or the weight pattern)
or pull in an unmaintained package. Nuban.NET is a small, correct, test-vector-verified
implementation of that one algorithm.

This library deliberately does not ship a registry of CBN bank codes. Bank codes change, new
banks and fintechs get licensed, and baking a bank list into a NuGet package guarantees it goes
stale. Nuban.NET expects the caller to supply the bank code, the same way a payment provider's
API or a bank-selection dropdown already does.

## Install

```
dotnet add package Nuban.Net
```

## Usage

### Quick validity check

```csharp
using Nuban;

bool ok = NubanValidator.IsValid("011", "0000014579");
// true - First Bank (011), matches the CBN's own published worked example
```

### Structured result, for showing the user why validation failed

```csharp
using Nuban;

NubanValidationResult result = NubanValidator.Validate("011", "0000014570");

if (!result.IsValid)
{
    Console.WriteLine($"{result.FailureReason}: expected check digit {result.ComputedCheckDigit}, " +
                       $"got {result.ProvidedCheckDigit}");
    // CheckDigitMismatch: expected check digit 9, got 0
}
```

### Computing a check digit while generating an account number

```csharp
using Nuban;

int checkDigit = NubanValidator.ComputeCheckDigit(bankCode: "058", serialNumber: "225647583");
string accountNumber = "225647583" + checkDigit;
```

## API

| Member | Purpose |
|---|---|
| `NubanValidator.IsValid(string bankCode, string accountNumber)` | Quick pass/fail check |
| `NubanValidator.Validate(string bankCode, string accountNumber)` | Structured `NubanValidationResult`: valid flag, extracted serial number, provided vs. computed check digit, and a `NubanValidationFailureReason` |
| `NubanValidator.ComputeCheckDigit(string bankCode, string serialNumber)` | The raw check digit (0-9) for a bank code and 9-digit serial; throws `ArgumentException` on malformed input rather than returning a sentinel, since every value 0-9 is a legitimate check digit |
| `NubanFormat` | The structural constants (`BankCodeLength`, `SerialNumberLength`, `CheckDigitLength`, `AccountNumberLength`) used to build the format, exposed for callers writing their own input masks or pre-checks |

`Validate` and `IsValid` never throw for malformed input; a null, empty, wrong-length, or
non-numeric bank code or account number simply comes back as `IsValid == false` with a specific
`NubanValidationFailureReason`. `ComputeCheckDigit` throws, because it is the lower-level
building block for callers who already know their input is well-formed.

## The algorithm

A NUBAN account number is 10 digits: a 9-digit account serial number followed by one check
digit. The check digit is computed from the CBN's 3-digit bank code for the receiving bank,
concatenated with the 9-digit serial number, using this procedure:

1. Multiply each of the 12 digits (3-digit bank code + 9-digit serial) by the repeating weight
   pattern `3, 7, 3` and sum the products.
2. Reduce the sum modulo 10.
3. Subtract that remainder from 10. If the result is 10, the check digit is 0.

Verified against the Central Bank of Nigeria's own published worked example (First Bank, bank
code `011`, serial `000001457`, sum `81`, remainder `1`, check digit `9`, giving NUBAN
`0000014579`), plus a second published example that exercises the remainder-zero branch (serial
`000000022`, sum `30`, remainder `0`, check digit `0`). Both are embedded as fixtures in the test
suite and asserted exactly, alongside additional algorithm-consistent examples generated with the
same verified algorithm to cover every possible check digit 0 through 9.

### What is not implemented

Some secondary sources describe a newer 16-digit NUBAN variant with a 6-digit institution code
(used for Other Financial Institutions such as mobile money operators, alongside the classic
3-digit Deposit Money Bank code). The CBN's own PDF for this revision blocks automated retrieval,
and every worked example for the 16-digit variant found in secondary sources failed to reconcile
against the stated algorithm when recomputed by hand. Rather than guess, Nuban.NET does not
implement or claim support for that variant. Only the classic, CBN-worked-example-verified
10-digit NUBAN algorithm ships.

## Dependencies and AOT

Zero runtime NuGet dependencies. The entire implementation is plain string and character
arithmetic with no reflection, no `System.Text.Json`, no I/O; it is trivially trimmable and
Native AOT compatible.

## License

MIT. See [LICENSE](LICENSE).
