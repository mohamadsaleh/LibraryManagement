using LibraryManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Endpoints.Students;

public static class GetStudent
{
    public static void MapGetStudent(this WebApplication app)
    {
        app.MapGet("/api/students/{id}", async (int id, ApplicationDbContext db) =>
        {
            var student = await db.Students.FindAsync(id);
            if (student is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(student);
        })
        .RequireEndpointName()
        .WithName("GetStudent")
        .WithOpenApi();
    }
}
