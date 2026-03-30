namespace DistributedShortener.Domain.Exceptions;

// Domain/Exceptions/ShortCodeNotFoundException.cs
public sealed class ShortCodeNotFoundException : Exception
{
    public ShortCodeNotFoundException(string code)
        : base($"Short code '{code}' was not found or is no longer active.")
    {
    }
}