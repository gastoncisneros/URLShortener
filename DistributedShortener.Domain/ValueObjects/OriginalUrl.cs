namespace DistributedShortener.Domain.ValueObjects;

/// <summary>
/// Its only responsibility is make sure there is no invalid URL in the domain
/// </summary>
public sealed record OriginalUrl
{
    public string Value { get; }
    private const int MaxLength = 2048;

    private OriginalUrl(string value) => Value = value;

    public static OriginalUrl From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException("URL cannot be empty");

        if (value.Length > MaxLength)
            throw new InvalidOperationException($"URL exceeds {MaxLength} characters");

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            (uri.Scheme != "http" && uri.Scheme != "https"))
            throw new InvalidOperationException($"'{value}' is not a valid URL");

        return new OriginalUrl(value);
    }
}