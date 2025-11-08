using LibraryManagement;
using LibraryManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Endpoints.Books;

public static class GetBooks
{
    public static void MapGetBooks(this WebApplication app)
    {
        app.MapGet("/api/books", async (ApplicationDbContext db) =>
        {
            var books = await db.Books.ToListAsync();
            return Results.Ok(books);
        })
        .RequireEndpointName()
        .WithName("GetBooks")
        .WithOpenApi()
        .WithMetadata(new AccessRightAttribute("Books:List", "اجازه مشاهده لیست کتاب ها در سیستم"));
    }
}