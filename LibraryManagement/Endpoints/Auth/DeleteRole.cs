using LibraryManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Endpoints.Auth;

public static class DeleteRole
{
    public static void MapDeleteRole(this WebApplication app)
    {
        app.MapDelete("/api/roles/{id}", async (int id, ApplicationDbContext db) =>
        {
            var role = await db.Roles.FindAsync(id);
            if (role is null)
            {
                return Results.NotFound();
            }

            // Check if role is being used by any users
            var usersWithRole = await db.UserHasRoles.AnyAsync(uhr => uhr.RoleId == id);
            if (usersWithRole)
            {
                return Results.BadRequest("Cannot delete role that is assigned to users");
            }

            db.Roles.Remove(role);
            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .RequireEndpointName()
        .WithName("DeleteRole")
        .WithOpenApi();
    }
}