using LibraryManagement;
using LibraryManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Endpoints.Students;

public static class CreateStudent
{
    public static void MapCreateStudent(this WebApplication app)
    {
        app.MapPost("/api/students", async ([FromBody] CreateStudentRequest request, ApplicationDbContext db) =>
        {
            var student = new Student
            {
                FirstName = request.FirstName,
                LastName = request.LastName
            };

            db.Students.Add(student);
            await db.SaveChangesAsync();

            return Results.Created($"/api/students/{student.Id}", student);
        })
        .RequireEndpointName()
        .WithName("CreateStudent")
        .WithOpenApi();
    }
}

public record CreateStudentRequest(string FirstName, string LastName);
