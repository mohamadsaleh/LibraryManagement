using LibraryManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Endpoints.Auth;

public static class GetRoles
{
    public static void MapGetRoles(this WebApplication app)
    {
        app.MapGet("/api/roles", async (ApplicationDbContext db) =>
        {
            var roles = await db.Roles.ToListAsync();
            return Results.Ok(roles);
        })
        .RequireEndpointName()
        .WithName("GetRoles")
        .WithOpenApi();
    }
}
