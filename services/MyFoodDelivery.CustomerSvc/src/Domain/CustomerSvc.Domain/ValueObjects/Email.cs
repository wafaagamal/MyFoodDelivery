using System;
using Volo.Abp.Domain.Values;

namespace CustomerSvc.Domain.ValueObjects;

/// <summary>
/// Value object representing a valid email address.
/// Ensures email format is valid and normalized.
/// </summary>
public class Email : ValueObject
{
    public string Value { get; private set; }

    private Email() { } // EF Core

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty.", nameof(value));

        var trimmed = value.Trim();
        
        if (!IsValidFormat(trimmed))
            throw new ArgumentException("Invalid email format.", nameof(value));

        Value = trimmed.ToLowerInvariant();
    }

    private static bool IsValidFormat(string email)
    {
        if (email.Length < 3 || email.Length > 254)
            return false;

        var atIndex = email.IndexOf('@');
        if (atIndex <= 0 || atIndex >= email.Length - 1)
            return false;

        var dotIndex = email.LastIndexOf('.');
        if (dotIndex <= atIndex + 1 || dotIndex >= email.Length - 1)
            return false;

        // No consecutive dots
        if (email.Contains(".."))
            return false;

        return true;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
