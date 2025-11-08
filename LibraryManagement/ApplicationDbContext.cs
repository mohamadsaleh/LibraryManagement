using LibraryManagement.Configuration;
using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<Right> Rights { get; set; } = null!;
    public DbSet<UserHasRole> UserHasRoles { get; set; } = null!;
    public DbSet<RoleHasRight> RoleHasRights { get; set; } = null!;
    public DbSet<Student> Students { get; set; } = null!;
    public DbSet<Book> Books { get; set; } = null!;
    public DbSet<Loan> Loans { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new User_Config());
        modelBuilder.ApplyConfiguration(new Role_Config());
        modelBuilder.ApplyConfiguration(new Right_Config());
        modelBuilder.ApplyConfiguration(new UserHasRole_Config());
        modelBuilder.ApplyConfiguration(new RoleHasRight_Config());
        modelBuilder.ApplyConfiguration(new Student_Config());
        modelBuilder.ApplyConfiguration(new Book_Config());
        modelBuilder.ApplyConfiguration(new Loan_Config());

        // Seed data
        //modelBuilder.Entity<Right>().HasData(
        //    new Right { Id = 1, Name = "api/auth/login" },
        //    new Right { Id = 2, Name = "api/students" },
        //    new Right { Id = 3, Name = "api/students/{id}" },
        //    new Right { Id = 4, Name = "api/books" },
        //    new Right { Id = 5, Name = "api/books/{id}" },
        //    new Right { Id = 6, Name = "api/loans/borrow" },
        //    new Right { Id = 7, Name = "api/loans/return/{loanId}" },
        //    new Right { Id = 8, Name = "api/users" },
        //    new Right { Id = 9, Name = "api/users/{id}" }
        //);

        //modelBuilder.Entity<Role>().HasData(
        //    new Role { Id = 1, Name = "FullAccess" }
        //);

        //modelBuilder.Entity<User>().HasData(
        //    new User
        //    {
        //        Id = 1,
        //        DisplayName = "Full Admin",
        //        Username = "FullAdmin",
        //        PasswordHash = "$2a$11$example.hash.here" // Will be replaced with actual hash
        //    }
        //);

        //modelBuilder.Entity<RoleHasRight>().HasData(
        //    new RoleHasRight { Id = 1, RoleId = 1, RightId = 1 },
        //    new RoleHasRight { Id = 2, RoleId = 1, RightId = 2 },
        //    new RoleHasRight { Id = 3, RoleId = 1, RightId = 3 },
        //    new RoleHasRight { Id = 4, RoleId = 1, RightId = 4 },
        //    new RoleHasRight { Id = 5, RoleId = 1, RightId = 5 },
        //    new RoleHasRight { Id = 6, RoleId = 1, RightId = 6 },
        //    new RoleHasRight { Id = 7, RoleId = 1, RightId = 7 },
        //    new RoleHasRight { Id = 8, RoleId = 1, RightId = 8 },
        //    new RoleHasRight { Id = 9, RoleId = 1, RightId = 9 }
        //);

        //modelBuilder.Entity<UserHasRole>().HasData(
        //    new UserHasRole { Id = 1, UserId = 1, RoleId = 1 }
        //);
    }
}