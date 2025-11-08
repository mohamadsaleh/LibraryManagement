using LibraryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Endpoints.Dev;

public static class CreateRole
{
    public static void MapCreateRole(this WebApplication app)
    {
        app.MapPost("/api/Dev/CreateRole", async ([FromBody] CreateRoleRequest request, ApplicationDbContext db) =>
        {
            // Check if a role with the same name already exists
            if (await db.Roles.AnyAsync(r => r.Name == request.Name))
            {
                return Results.Conflict(new { message = $"A role with the name '{request.Name}' already exists." });
            }

            var role = new Role
            {
                Name = request.Name
            };

            db.Roles.Add(role);
            await db.SaveChangesAsync();

            return Results.Created($"/api/Dev/roles/{role.Id}", role);
        })
        .WithName("CreateRoleDev")
        .WithTags("Dev")
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Create a new role.",
            Description = "This endpoint allows for the manual creation of a new role in the system."
        });
    }
}

/// <summary>
/// Represents the request to create a new role.
/// </summary>
/// <param name="Name">The unique name of the role.</param>
public record CreateRoleRequest(string Name);