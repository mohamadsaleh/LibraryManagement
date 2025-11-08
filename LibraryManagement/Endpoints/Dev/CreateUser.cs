using LibraryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace LibraryManagement.Endpoints.Dev;

public static class CreateUser
{
    public static void MapCreateUser(this WebApplication app)
    {
        app.MapPost("/api/Dev/CreateUser", async ([FromBody] CreateUserRequest request, ApplicationDbContext db) =>
        {
            if (await db.Users.AnyAsync(u => u.Username == request.Username))
            {
                return Results.Conflict(new { message = $"Username '{request.Username}' already exists." });
            }

            var user = new User
            {
                DisplayName = request.DisplayName,
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            return Results.Created($"User with Id: {user.Id} Created.", user);
        })
        .WithName("CreateUserDev")
        .WithTags("Dev")
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Create a new user for development purposes.",
            Description = "This endpoint creates a new user without requiring authorization. It's intended for testing and setup."
        });
    }
}
public record CreateUserRequest(string DisplayName, string Username, string Password);
