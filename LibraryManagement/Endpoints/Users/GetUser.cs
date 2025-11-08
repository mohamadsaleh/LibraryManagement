using LibraryManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Endpoints.Users;

public static class GetUser
{
    public static void MapGetUser(this WebApplication app)
    {
        app.MapGet("/api/users/{id}", async (int id, ApplicationDbContext db) =>
        {
            var user = await db.Users
                .Include(u => u.UserHasRoles)
                .ThenInclude(uhr => uhr.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(user);
        })
        .RequireEndpointName()
        .WithName("GetUser")
        .WithOpenApi();
    }
}
