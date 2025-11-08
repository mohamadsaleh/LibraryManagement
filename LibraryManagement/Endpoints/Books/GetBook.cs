using LibraryManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Endpoints.Books;

public static class GetBook
{
    public static void MapGetBook(this WebApplication app)
    {
        app.MapGet("/api/books/{id}", async (int id, ApplicationDbContext db) =>
        {
            var book = await db.Books.FindAsync(id);
            if (book is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(book);
        })
        .RequireEndpointName()
        .WithName("GetBook")
        .WithOpenApi();
    }
}
