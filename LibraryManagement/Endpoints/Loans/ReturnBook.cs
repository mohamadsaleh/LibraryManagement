using LibraryManagement;
using LibraryManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Endpoints.Loans;

public static class ReturnBook
{
    public static void MapReturnBook(this WebApplication app)
    {
        app.MapPost("/api/loans/return/{loanId}", async (int loanId, ApplicationDbContext db) =>
        {
            var loan = await db.Loans
                .Include(l => l.Book)
                .FirstOrDefaultAsync(l => l.Id == loanId && l.ReturnDate == null);

            if (loan is null)
            {
                return Results.NotFound("Active loan not found");
            }

            loan.ReturnDate = DateTime.UtcNow;
            loan.Book.IsAvailable = true;

            await db.SaveChangesAsync();

            return Results.Ok(loan);
        })
        .RequireEndpointName()
        .WithName("ReturnBook")
        .WithOpenApi();
    }
}
