using LibraryManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Endpoints.Loans;

public static class DeleteLoan
{
    public static void MapDeleteLoan(this WebApplication app)
    {
        app.MapDelete("/api/loans/{id}", async (int id, ApplicationDbContext db) =>
        {
            var loan = await db.Loans.FindAsync(id);
            if (loan is null)
            {
                return Results.NotFound();
            }

            db.Loans.Remove(loan);
            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .RequireEndpointName()
        .WithName("DeleteLoan")
        .WithOpenApi();
    }
}
