using LibraryManagement;
using LibraryManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Endpoints.Users;

public static class CreateUser
{
    public static void MapCreateUser(this WebApplication app)
    {
        app.MapPost("/api/users", async ([FromBody] CreateUserRequest request, ApplicationDbContext db) =>
        {
            if (await db.Users.AnyAsync(u => u.Username == request.Username))
            {
                return Results.BadRequest("Username already exists");
            }

            var user = new User
            {
                DisplayName = request.DisplayName,
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            // Add role if provided
            if (request.RoleId.HasValue)
            {
                var userHasRole = new UserHasRole
                {
                    UserId = user.Id,
                    RoleId = request.RoleId.Value
                };
                db.UserHasRoles.Add(userHasRole);
                await db.SaveChangesAsync();
            }

            return Results.Created($"/api/users/{user.Id}", user);
        })
        .RequireEndpointName()
        .WithName("CreateUser")
        .WithOpenApi();
    }
}

public record CreateUserRequest(string DisplayName, string Username, string Password, int? RoleId);
