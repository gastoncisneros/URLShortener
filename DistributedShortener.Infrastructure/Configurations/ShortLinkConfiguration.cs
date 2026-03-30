using DistributedShortener.Domain.Aggregates;
using DistributedShortener.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DistributedShortener.Infrastructure.Configurations;

/// <summary>
/// Why do we need ShortLinkConfiguration?
/// The domain doesn't have to know anything about EF Core
/// So we use this configuration as a bridge through Domain and EF Core
///
/// DOMAIN (No EF Core) => ShortLinkConfiguration => Infrastructure
/// </summary>
public sealed class ShortLinkConfiguration : IEntityTypeConfiguration<ShortLink>
{
    public void Configure(EntityTypeBuilder<ShortLink> builder)
    {
        builder.ToTable("short_links");

        builder.HasKey(x => x.Id);

        // Value Object ShortCode → column "code"
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(6)
            .IsRequired()
            .HasConversion(
                code => code.Value,           // ShortCode → string (para guardar)
                value => ShortCode.From(value) // string → ShortCode (para leer)
            );

        // Value Object OriginalUrl → column "original_url"
        builder.Property(x => x.OriginalUrl)
            .HasColumnName("original_url")
            .HasMaxLength(2048)
            .IsRequired()
            .HasConversion(
                url => url.Value,
                value => OriginalUrl.From(value)
            );

        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired(false);

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasDatabaseName("ix_short_links_code");

        // Domain events are not persisted in this table
        // We'll use a dedicated table
        builder.Ignore(x => x.DomainEvents);
    }
}