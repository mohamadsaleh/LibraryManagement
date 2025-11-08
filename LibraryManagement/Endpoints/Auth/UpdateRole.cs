using LibraryManagement;
using LibraryManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Endpoints.Auth;

public static class UpdateRole
{
    public static void MapUpdateRole(this WebApplication app)
    {
        app.MapPut("/api/roles/{id}", async (int id, [FromBody] UpdateRoleRequest request, ApplicationDbContext db) =>
        {
            var role = await db.Roles.FindAsync(id);
            if (role is null)
            {
                return Results.NotFound();
            }

            role.Name = request.Name;

            // Update rights if provided
            if (request.RightIds != null)
            {
                // Remove existing rights
                var existingRights = await db.RoleHasRights.Where(rhr => rhr.RoleId == id).ToListAsync();
                db.RoleHasRights.RemoveRange(existingRights);

                // Add new rights
                foreach (var rightId in request.RightIds)
                {
                    var roleHasRight = new RoleHasRight
                    {
                        RoleId = id,
                        RightId = rightId
                    };
                    db.RoleHasRights.Add(roleHasRight);
                }
            }

            await db.SaveChangesAsync();

            return Results.Ok(role);
        })
        .RequireEndpointName()
        .WithName("UpdateRole")
        .WithOpenApi();
    }
}

public record UpdateRoleRequest(string Name, List<int>? RightIds);