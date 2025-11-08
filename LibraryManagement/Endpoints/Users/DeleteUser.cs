using LibraryManagement;
using LibraryManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Endpoints.Users;

public static class DeleteUser
{
    public static void MapDeleteUser(this WebApplication app)
    {
        app.MapDelete("/api/users/{id}", async (int id, ApplicationDbContext db) =>
        {
            var user = await db.Users.FindAsync(id);
            if (user is null)
            {
                return Results.NotFound();
            }

            db.Users.Remove(user);
            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .RequireEndpointName()
        .WithName("DeleteUser")
        .WithOpenApi();
    }
}
