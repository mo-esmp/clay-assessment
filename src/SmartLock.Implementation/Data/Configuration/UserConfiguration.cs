using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartLock.Domain.Users;

namespace SmartLock.Implementation.Data.Configuration;

internal class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.Property(p => p.FirstName).HasMaxLength(32).IsUnicode().IsRequired();
        builder.Property(p => p.LastName).HasMaxLength(32).IsUnicode().IsRequired();

        builder.HasMany(e => e.UserRoles)
            .WithOne(e => e.User)
            .HasForeignKey(ur => ur.UserId)
            .IsRequired();
    }
}