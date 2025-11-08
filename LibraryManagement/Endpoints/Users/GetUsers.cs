using LibraryManagement;
using LibraryManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Endpoints.Users;

public static class GetUsers
{
    public static void MapGetUsers(this WebApplication app)
    {
        app.MapGet("/api/users", async (ApplicationDbContext db) =>
        {
            var users = await db.Users
                .Include(u => u.UserHasRoles)
                .ThenInclude(ur => ur.Role)
                .ToListAsync();
            return Results.Ok(users);
        })
        .RequireEndpointName()
        .WithName("GetUsers")
        .WithOpenApi();
    }
}
