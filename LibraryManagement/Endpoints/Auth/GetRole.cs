using LibraryManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Endpoints.Auth;

public static class GetRole
{
    public static void MapGetRole(this WebApplication app)
    {
        app.MapGet("/api/roles/{id}", async (int id, ApplicationDbContext db) =>
        {
            var role = await db.Roles
                .Include(r => r.RoleHasRights)
                .ThenInclude(rhr => rhr.Right)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(role);
        })
        .RequireEndpointName()
        .WithName("GetRole")
        .WithOpenApi();
    }
}
