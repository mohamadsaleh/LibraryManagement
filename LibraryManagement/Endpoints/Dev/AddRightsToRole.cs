
using LibraryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Endpoints.Dev;

public static class AddRightsToRole
{
    public static void MapAddRightsToRole(this WebApplication app)
    {
        app.MapPost("/api/Dev/AddRightsToRole", async ([FromBody] AddRightsToRoleRequest request, ApplicationDbContext db) =>
        {
            // Find the role
            var role = await db.Roles.FindAsync(request.RoleId);

            if (role == null)
            {
                return Results.NotFound(new { message = $"Role with ID {request.RoleId} not found." });
            }

            // Find the rights that should be assigned based on the provided IDs
            var rightsToAssign = await db.Rights
                .Where(r => request.RightIds.Contains(r.Id))
                .ToListAsync();

            // Optional: Check if all requested right IDs were found
            if (rightsToAssign.Count != request.RightIds.Count)
            {
                var foundIds = rightsToAssign.Select(r => r.Id);
                var notFoundIds = request.RightIds.Except(foundIds);
                return Results.BadRequest(new { message = $"The following Right IDs were not found: {string.Join(", ", notFoundIds)}" });
            }

            // Find and remove all existing rights for this role
            var existingRights = await db.RoleHasRights
                .Where(rhr => rhr.RoleId == request.RoleId)
                .ToListAsync();

            db.RoleHasRights.RemoveRange(existingRights);

            // Create new RoleHasRight entries for the new set of rights
            foreach (var right in rightsToAssign)
            {
                db.RoleHasRights.Add(new RoleHasRight { RoleId = role.Id, RightId = right.Id });
            }

            await db.SaveChangesAsync();

            return Results.Ok(new { message = $"Successfully assigned {rightsToAssign.Count} rights to role '{role.Name}'." });
        })
        .WithName("AddRightsToRoleDev")
        .WithTags("Dev")
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Assign a set of rights to a specific role.",
            Description = "This endpoint replaces all existing rights of a role with the new set of rights provided."
        });
    }
}

public class AddRightsToRoleRequest
{
    public int RoleId { get; set; }
    public List<int> RightIds { get; set; } = new();
}
