using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryPlatform.Infrastructure.Persistence.Configurations;

public sealed class UserAuthorizationGroupConfiguration
    : IEntityTypeConfiguration<UserAuthorizationGroup>
{
    public void Configure(EntityTypeBuilder<UserAuthorizationGroup> builder)
    {
        builder.ToTable("UserAuthorizationGroups");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.AuthorizationGroupId)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.UserId,
            x.AuthorizationGroupId
        })
            .IsUnique();

        builder.HasIndex(x => x.AuthorizationGroupId);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<AuthorizationGroup>()
            .WithMany(x => x.UserGroups)
            .HasForeignKey(x => x.AuthorizationGroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
