using LibraryManagement;
using LibraryManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Endpoints.Loans;

public static class BorrowBook
{
    public static void MapBorrowBook(this WebApplication app)
    {
        app.MapPost("/api/loans/borrow", async ([FromBody] BorrowBookRequest request, ApplicationDbContext db) =>
        {
            var book = await db.Books.FindAsync(request.BookId);
            if (book is null)
            {
                return Results.NotFound("Book not found");
            }

            if (!book.IsAvailable)
            {
                return Results.BadRequest("Book is not available");
            }

            var student = await db.Students.FindAsync(request.StudentId);
            if (student is null)
            {
                return Results.NotFound("Student not found");
            }

            var loan = new Loan
            {
                BookId = request.BookId,
                StudentId = request.StudentId,
                BorrowDate = DateTime.UtcNow
            };

            book.IsAvailable = false;

            db.Loans.Add(loan);
            await db.SaveChangesAsync();

            return Results.Created($"/api/loans/{loan.Id}", loan);
        })
        .RequireEndpointName()
        .WithName("BorrowBook")
        .WithOpenApi();
    }
}

public record BorrowBookRequest(int BookId, int StudentId);
