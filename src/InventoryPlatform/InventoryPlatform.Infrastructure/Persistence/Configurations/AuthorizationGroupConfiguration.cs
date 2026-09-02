using InventoryPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryPlatform.Infrastructure.Persistence.Configurations;

public sealed class AuthorizationGroupConfiguration
    : IEntityTypeConfiguration<AuthorizationGroup>
{
    public void Configure(EntityTypeBuilder<AuthorizationGroup> builder)
    {
        builder.ToTable("AuthorizationGroups");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.Navigation(x => x.Capabilities)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(x => x.UserGroups)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
