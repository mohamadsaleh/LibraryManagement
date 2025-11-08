using LibraryManagement;
using LibraryManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Endpoints.Auth;

public static class CreateRole
{
    public static void MapCreateRole(this WebApplication app)
    {
        app.MapPost("/api/roles", async ([FromBody] CreateRoleRequest request, ApplicationDbContext db) =>
        {
            if (await db.Roles.AnyAsync(r => r.Name == request.Name))
            {
                return Results.BadRequest("Role name already exists");
            }

            var role = new Role
            {
                Name = request.Name
            };

            db.Roles.Add(role);
            await db.SaveChangesAsync();

            // Add rights if provided
            if (request.RightIds != null && request.RightIds.Any())
            {
                foreach (var rightId in request.RightIds)
                {
                    var roleHasRight = new RoleHasRight
                    {
                        RoleId = role.Id,
                        RightId = rightId
                    };
                    db.RoleHasRights.Add(roleHasRight);
                }
                await db.SaveChangesAsync();
            }

            return Results.Created($"/api/roles/{role.Id}", role);
        })
        .RequireEndpointName()
        .WithName("CreateRole")
        .WithOpenApi();
    }
}

public record CreateRoleRequest(string Name, List<int>? RightIds);
