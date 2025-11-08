using LibraryManagement;
using LibraryManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Endpoints.Students;

public static class UpdateStudent
{
    public static void MapUpdateStudent(this WebApplication app)
    {
        app.MapPut("/api/students/{id}", async (int id, [FromBody] UpdateStudentRequest request, ApplicationDbContext db) =>
        {
            var student = await db.Students.FindAsync(id);
            if (student is null)
            {
                return Results.NotFound();
            }

            student.FirstName = request.FirstName;
            student.LastName = request.LastName;

            await db.SaveChangesAsync();

            return Results.Ok(student);
        })
        .RequireEndpointName()
        .WithName("UpdateStudent")
        .WithOpenApi();
    }
}

public record UpdateStudentRequest(string FirstName, string LastName);
