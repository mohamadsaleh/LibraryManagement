
using LibraryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Endpoints.Dev
{
    public static class AddRolesToUser
    {
        public static void MapAddRolesToUser(this WebApplication app)
        {
            app.MapPost("/api/Dev/AddRolesToUser", async ([FromBody] AddRolesToUserRequest request, ApplicationDbContext db) =>
            {
                // Find the user by their ID
                var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId);

                if (user == null)
                {
                    return Results.NotFound(new { message = $"User with ID {request.UserId} not found." });
                }

                // Find the roles that should be assigned based on the provided IDs
                var rolesToAssign = await db.Roles
                    .Where(r => request.RoleIds.Contains(r.Id))
                    .ToListAsync();

                // Optional: Check if all requested role IDs were found
                if (rolesToAssign.Count != request.RoleIds.Count)
                {
                    var foundIds = rolesToAssign.Select(r => r.Id);
                    var notFoundIds = request.RoleIds.Except(foundIds);
                    return Results.BadRequest(new { message = $"The following Role IDs were not found: {string.Join(", ", notFoundIds)}" });
                }

                // Find and remove all existing role assignments for this user from the UserHasRoles table
                var existingAssignments = await db.UserHasRoles
                    .Where(uhr => uhr.UserId == request.UserId)
                    .ToListAsync();

                if (existingAssignments.Any())
                {
                    db.UserHasRoles.RemoveRange(existingAssignments);
                }

                // Create new assignments in the UserHasRoles table
                foreach (var role in rolesToAssign)
                {
                    var newAssignment = new UserHasRole
                    {
                        UserId = user.Id,
                        RoleId = role.Id
                    };
                    db.UserHasRoles.Add(newAssignment);
                }

                await db.SaveChangesAsync();

                return Results.Ok(new { message = $"Successfully assigned {rolesToAssign.Count} roles to user '{user.Username}'." });
            })
            .WithName("AddRolesToUserDev")
            .WithTags("Dev")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Assign a set of roles to a specific user.",
                Description = "This endpoint replaces all existing roles of a user with the new set of roles provided."
            });
        }
    }

    public record AddRolesToUserRequest(int UserId, List<int> RoleIds);
}
