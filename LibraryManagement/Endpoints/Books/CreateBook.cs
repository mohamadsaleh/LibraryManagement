using LibraryManagement;
using LibraryManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Endpoints.Books;

public static class CreateBook
{
    public static void MapCreateBook(this WebApplication app)
    {
        app.MapPost("/api/books", async ([FromBody] CreateBookRequest request, ApplicationDbContext db) =>
        {
            var book = new Book
            {
                Title = request.Title,
                Author = request.Author,
                IsAvailable = true
            };

            db.Books.Add(book);
            await db.SaveChangesAsync();

            return Results.Created($"/api/books/{book.Id}", book);
        })
        .RequireEndpointName()
        .WithName("CreateBook")
        .WithOpenApi()
        .WithMetadata(new AccessRightAttribute("Books:Create", "اجازه ایجاد یک کتاب جدید در سیستم"));
    }
}

public record CreateBookRequest(string Title, string Author);