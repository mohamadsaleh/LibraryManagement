using LibraryManagement;
using LibraryManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Endpoints.Books;

public static class DeleteBook
{
    public static void MapDeleteBook(this WebApplication app)
    {
        app.MapDelete("/api/books/{id}", async (int id, ApplicationDbContext db) =>
        {
            var book = await db.Books.FindAsync(id);
            if (book is null)
            {
                return Results.NotFound();
            }

            db.Books.Remove(book);
            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .RequireEndpointName()
        .WithName("DeleteBook")
        .WithOpenApi()
        .WithMetadata(new AccessRightAttribute("Books:Delete", "????? ??? ?? ???? ?? ?????"));
    }
}
