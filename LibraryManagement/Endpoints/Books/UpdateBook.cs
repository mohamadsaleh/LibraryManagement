using LibraryManagement;
using LibraryManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Endpoints.Books;

public static class UpdateBook
{
    public static void MapUpdateBook(this WebApplication app)
    {
        app.MapPut("/api/books/{id}", async (int id, [FromBody] UpdateBookRequest request, ApplicationDbContext db) =>
        {
            var book = await db.Books.FindAsync(id);
            if (book is null)
            {
                return Results.NotFound();
            }

            book.Title = request.Title;
            book.Author = request.Author;

            await db.SaveChangesAsync();

            return Results.Ok(book);
        })
        .RequireEndpointName()
        .WithName("UpdateBook")
        .WithOpenApi()
        .WithMetadata(new AccessRightAttribute("Books:Update", "????? ????? ?? ???? ???? ?? ?????"));
    }
}

public record UpdateBookRequest(string Title, string Author);
