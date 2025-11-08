using LibraryManagement;
using LibraryManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Endpoints.Users;

public static class UpdateUser
{
    public static void MapUpdateUser(this WebApplication app)
    {
        app.MapPut("/api/users/{id}", async (int id, [FromBody] UpdateUserRequest request, ApplicationDbContext db) =>
        {
            var user = await db.Users.FindAsync(id);
            if (user is null)
            {
                return Results.NotFound();
            }

            user.DisplayName = request.DisplayName;
            user.Username = request.Username;

            if (!string.IsNullOrEmpty(request.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            }

            // Update role if provided
            if (request.RoleId.HasValue)
            {
                // Remove existing role assignments
                var existingRoles = await db.UserHasRoles.Where(uhr => uhr.UserId == id).ToListAsync();
                db.UserHasRoles.RemoveRange(existingRoles);

                // Add new role assignment
                var userHasRole = new UserHasRole
                {
                    UserId = id,
                    RoleId = request.RoleId.Value
                };
                db.UserHasRoles.Add(userHasRole);
            }

            await db.SaveChangesAsync();

            return Results.Ok(user);
        })
        .RequireEndpointName()
        .WithName("UpdateUser")
        .WithOpenApi();
    }
}

public record UpdateUserRequest(string DisplayName, string Username, string? Password, int? RoleId);
