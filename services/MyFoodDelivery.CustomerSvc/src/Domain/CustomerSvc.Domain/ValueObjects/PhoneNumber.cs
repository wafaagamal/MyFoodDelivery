using System;
using System.Linq;
using Volo.Abp.Domain.Values;

namespace CustomerSvc.Domain.ValueObjects;

/// <summary>
/// Value object representing a valid phone number.
/// Stores normalized format for consistency.
/// </summary>
public class PhoneNumber : ValueObject
{
    public string Value { get; private set; }
    public string? CountryCode { get; private set; }

    private PhoneNumber() { } // EF Core

    public PhoneNumber(string value, string? countryCode = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Phone number cannot be empty.", nameof(value));

        var normalized = NormalizeNumber(value);
        
        if (!IsValidFormat(normalized))
            throw new ArgumentException("Invalid phone number format.", nameof(value));

        Value = normalized;
        CountryCode = countryCode?.Trim();
    }

    private static string NormalizeNumber(string value)
    {
        // Remove all non-digit characters except leading +
        var hasPlus = value.TrimStart().StartsWith('+');
        var digits = new string(value.Where(char.IsDigit).ToArray());
        return hasPlus ? $"+{digits}" : digits;
    }

    private static bool IsValidFormat(string phone)
    {
        var digits = phone.Where(char.IsDigit).Count();
        return digits >= 7 && digits <= 15;
    }

    public string GetFullNumber()
    {
        if (string.IsNullOrEmpty(CountryCode))
            return Value;
        
        return Value.StartsWith('+') ? Value : $"+{CountryCode}{Value}";
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
        yield return CountryCode ?? string.Empty;
    }

    public override string ToString() => GetFullNumber();

    public static implicit operator string(PhoneNumber phone) => phone.Value;
}
