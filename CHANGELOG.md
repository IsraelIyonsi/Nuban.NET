# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2026-08-12

### Added

- `NubanValidator` static API: `IsValid(bankCode, accountNumber)`, `Validate(bankCode, accountNumber)`, and `ComputeCheckDigit(bankCode, serialNumber)`.
- `NubanValidationResult`: structured outcome with the valid flag, the extracted serial number, the provided check digit, the computed check digit, and a `NubanValidationFailureReason`.
- `NubanValidationFailureReason`: distinguishes a missing, wrong-length, or non-numeric bank code or account number from a genuine check digit mismatch.
- `NubanFormat`: public structural constants for the 10-digit NUBAN format.
- Central Bank of Nigeria modulus-10 check digit algorithm: repeating weight pattern 3, 7, 3 across the 3-digit bank code plus 9-digit serial number, summed, reduced modulo 10, subtracted from 10, with a result of 10 mapped to 0.
- Verified against two CBN-published worked examples (First Bank, bank code 011), embedded as test fixtures and asserted exactly, plus additional algorithm-consistent fixtures covering every check digit 0 through 9.
- Zero runtime dependencies; no bundled bank code registry, by design.

### Notes

- A newer 16-digit NUBAN variant with a 6-digit institution code is described in secondary sources but could not be verified against a primary CBN source or a reconcilable worked example; it is not implemented. See the README for details.
