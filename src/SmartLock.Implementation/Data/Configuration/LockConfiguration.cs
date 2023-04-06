using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartLock.Domain.Locks;

namespace SmartLock.Implementation.Data.Configuration;

internal class LockConfiguration : IEntityTypeConfiguration<LockEntity>
{
    public void Configure(EntityTypeBuilder<LockEntity> builder)
    {
        builder.Property(b => b.Name).HasMaxLength(128).IsUnicode(false).IsRequired();
        builder.Property(b => b.Name).HasMaxLength(128).IsUnicode(false).IsRequired();

        builder.HasMany(e => e.LockAccesses)
            .WithOne(e => e.Lock)
            .HasForeignKey(ur => ur.LockId)
            .IsRequired();
    }
}