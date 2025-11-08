using LibraryManagement;
using LibraryManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Endpoints.Students;

public static class DeleteStudent
{
    public static void MapDeleteStudent(this WebApplication app)
    {
        app.MapDelete("/api/students/{id}", async (int id, ApplicationDbContext db) =>
        {
            var student = await db.Students.FindAsync(id);
            if (student is null)
            {
                return Results.NotFound();
            }

            db.Students.Remove(student);
            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .RequireEndpointName()
        .WithName("DeleteStudent")
        .WithOpenApi();
    }
}
