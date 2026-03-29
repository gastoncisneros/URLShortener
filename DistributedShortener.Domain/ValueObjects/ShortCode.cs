using System.Text.RegularExpressions;

namespace DistributedShortener.Domain.ValueObjects;

/// <summary>
/// Same as OriginalUrl, its only responsibility is to guarantee that there is no invalid
/// short code in the domain
/// </summary>
public sealed record ShortCode
{
    public string Value { get; }
    
    private static readonly Regex ValidPattern = new (@"^[a-zA-Z0-9]{6}$", RegexOptions.Compiled);
    
    private ShortCode(string value) =>  Value = value;

    public static ShortCode From(string value)
    {
        return (string.IsNullOrWhiteSpace(value) || !ValidPattern.IsMatch(value))
            ? throw new InvalidOperationException(value) 
            : new ShortCode(value);
    }

    public static ShortCode Generate()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var code = new string(Enumerable.Range(0, 6)
            .Select(_ => chars[Random.Shared.Next(chars.Length)])
            .ToArray());
        return new ShortCode(code);
    }
}