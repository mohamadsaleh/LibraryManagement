using LibraryManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Endpoints.Loans;

public static class GetLoans
{
    public static void MapGetLoans(this WebApplication app)
    {
        app.MapGet("/api/loans", async (ApplicationDbContext db) =>
        {
            var loans = await db.Loans
                .Include(l => l.Student)
                .Include(l => l.Book)
                .ToListAsync();

            return Results.Ok(loans);
        })
        .RequireEndpointName()
        .WithName("GetLoans")
        .WithOpenApi();
    }
}