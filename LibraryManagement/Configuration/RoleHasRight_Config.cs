using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryManagement.Configuration;

public class RoleHasRight_Config : IEntityTypeConfiguration<RoleHasRight>
{
    public void Configure(EntityTypeBuilder<RoleHasRight> builder)
    {
        builder.HasKey(rr => rr.Id);
        builder.HasOne(rr => rr.Role).WithMany(r => r.RoleHasRights).HasForeignKey(rr => rr.RoleId);
        builder.HasOne(rr => rr.Right).WithMany(r => r.RoleHasRights).HasForeignKey(rr => rr.RightId);
        builder.HasIndex(rr => new { rr.RoleId, rr.RightId }).IsUnique();
    }
}