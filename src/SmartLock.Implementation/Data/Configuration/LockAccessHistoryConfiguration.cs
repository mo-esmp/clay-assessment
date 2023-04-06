using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartLock.Domain.Locks;

namespace SmartLock.Implementation.Data.Configuration;

internal class LockAccessHistoryConfiguration : IEntityTypeConfiguration<LockAccessHistoryEntity>
{
    public void Configure(EntityTypeBuilder<LockAccessHistoryEntity> builder)
    {
        builder.HasOne(e => e.Lock)
            .WithMany(e => e.LockAccesses)
            .HasForeignKey(ur => ur.LockId)
            .IsRequired();

        builder.HasOne(e => e.Requester)
            .WithMany()
            .HasForeignKey(ur => ur.RequesterId)
            .IsRequired();

        builder.HasOne(e => e.RequestForEmployee)
            .WithMany()
            .HasForeignKey(ur => ur.RequestForEmployeeId);
    }
}