
using LibraryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Endpoints.Dev;

public static class GetRolesByUserId
{
    public static void MapGetRolesByUserId(this WebApplication app)
    {
        app.MapGet("/api/Dev/GetRolesByUserId", async ([FromQuery] int userId, ApplicationDbContext db) =>
        {
            // Check if the user exists
            var userExists = await db.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                return Results.NotFound(new { message = $"User with ID {userId} not found." });
            }

            // Query to get roles for the user via the UserHasRole join table
            var roles = await db.UserHasRoles
                .Where(uhr => uhr.UserId == userId) // Filter by UserId
                .Select(uhr => uhr.Role)            // Select the associated Role
                .OrderBy(r => r.Name)
                .AsNoTracking()
                .ToListAsync();

            return Results.Ok(roles);
        })
        .WithName("GetRolesByUserIdDev")
        .WithTags("Dev")
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Get all roles for a specific user.",
            Description = "This endpoint retrieves a list of all roles assigned to a user by their ID."
        });
    }
}
