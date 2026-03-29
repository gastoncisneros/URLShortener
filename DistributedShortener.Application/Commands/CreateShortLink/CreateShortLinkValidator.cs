using FluentValidation;

namespace DistributedShortener.Application.Commands.CreateShortLink;

public sealed class CreateShortLinkValidator : AbstractValidator<CreateShortLinkCommand>
{
    public CreateShortLinkValidator()
    {
        RuleFor(x => x.OriginalUrl)
            .NotEmpty()
            .WithMessage("URL is required")
            .MaximumLength(2048)
            .WithMessage("URL cannot exceed 2048 characters")
            .Must(BeAValidUrl)
            .WithMessage("Must be a valid HTTP or HTTPS URL");
        
        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Expiry date must be in the future")
            .When(x => x.ExpiresAt.HasValue);
    }

    private static bool BeAValidUrl(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
        uri.Scheme is "http" or "https";
}