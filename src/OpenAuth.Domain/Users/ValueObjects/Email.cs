using System.Text.RegularExpressions;

namespace OpenAuth.Domain.Users.ValueObjects;

public sealed record Email
{
    public string Value { get; }
    
    public Email(string value)
    {
        if (!IsValid(value))
            throw new ArgumentException("Email is not valid.", nameof(value));

        Value = value;
    }

    public static bool IsValid(string email)
        => Regex.IsMatch(email, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
}