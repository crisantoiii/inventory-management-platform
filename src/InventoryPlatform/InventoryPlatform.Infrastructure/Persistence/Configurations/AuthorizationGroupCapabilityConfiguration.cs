using InventoryPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryPlatform.Infrastructure.Persistence.Configurations;

public sealed class AuthorizationGroupCapabilityConfiguration
    : IEntityTypeConfiguration<AuthorizationGroupCapability>
{
    public void Configure(EntityTypeBuilder<AuthorizationGroupCapability> builder)
    {
        builder.ToTable("AuthorizationGroupCapabilities");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AuthorizationGroupId)
            .IsRequired();

        builder.Property(x => x.CapabilityId)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.AuthorizationGroupId,
            x.CapabilityId
        })
            .IsUnique();

        builder.HasIndex(x => x.CapabilityId);

        builder.HasOne<AuthorizationGroup>()
            .WithMany(x => x.Capabilities)
            .HasForeignKey(x => x.AuthorizationGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Capability>()
            .WithMany(x => x.GroupCapabilities)
            .HasForeignKey(x => x.CapabilityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
