using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryManagement.Configuration;

public class UserHasRole_Config : IEntityTypeConfiguration<UserHasRole>
{
    public void Configure(EntityTypeBuilder<UserHasRole> builder)
    {
        builder.HasKey(ur => ur.Id);
        builder.HasOne(ur => ur.User).WithMany(u => u.UserHasRoles).HasForeignKey(ur => ur.UserId);
        builder.HasOne(ur => ur.Role).WithMany(r => r.UserHasRoles).HasForeignKey(ur => ur.RoleId);
        builder.HasIndex(ur => new { ur.UserId, ur.RoleId }).IsUnique();
    }
}