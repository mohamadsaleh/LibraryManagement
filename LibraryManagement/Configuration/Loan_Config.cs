using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryManagement.Configuration;

public class Loan_Config : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.HasKey(l => l.Id);
        builder.HasOne(l => l.Book).WithMany(b => b.Loans).HasForeignKey(l => l.BookId);
        builder.HasOne(l => l.Student).WithMany(s => s.Loans).HasForeignKey(l => l.StudentId);
        builder.Property(l => l.BorrowDate).IsRequired();
        builder.Property(l => l.ReturnDate).IsRequired(false);
    }
}