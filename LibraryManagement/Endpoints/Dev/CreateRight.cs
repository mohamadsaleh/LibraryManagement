using LibraryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Endpoints.Dev;

public static class CreateRight
{
    public static void MapCreateRight(this WebApplication app)
    {
        app.MapPost("/api/Dev/CreateRight", async ([FromBody] CreateRightRequest request, ApplicationDbContext db) =>
        {
            // Check for uniqueness of the Right Name
            if (await db.Rights.AnyAsync(r => r.Name == request.Name))
            {
                return Results.Conflict(new { message = $"A right with the name '{request.Name}' already exists." });
            }

            var right = new Right
            {
                Name = request.Name,
                Description = request.Description,
                Type = request.Type ?? "Manual" // Default type if not provided
            };

            db.Rights.Add(right);
            await db.SaveChangesAsync();

            return Results.Created($"/api/Dev/rights/{right.Id}", right);
        })
        .WithName("CreateRightDev")
        .WithTags("Dev")
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Create a new access right.",
            Description = "This endpoint allows for the manual creation of a new access right in the system."
        });
    }
}

/// <summary>
/// Represents the request to create a new access right.
/// </summary>
/// <param name="Name">The unique name of the right.</param>
/// <param name="Description">A user-friendly description of the right.</param>
/// <param name="Type">The type of the right (e.g., 'Endpoint', 'Manual'). Defaults to 'Manual' if not provided.</param>
public record CreateRightRequest(string Name, string Description, string? Type);
