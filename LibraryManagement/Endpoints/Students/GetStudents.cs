using LibraryManagement;
using LibraryManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Endpoints.Students;

public static class GetStudents
{
    public static void MapGetStudents(this WebApplication app)
    {
        app.MapGet("/api/students", async (ApplicationDbContext db) =>
        {
            var students = await db.Students.ToListAsync();
            return Results.Ok(students);
        })
        .RequireEndpointName()
        .WithName("GetStudents")
        .WithOpenApi();
    }
}
