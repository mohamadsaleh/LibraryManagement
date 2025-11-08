using LibraryManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LibraryManagement.Endpoints.Auth;

public static class GetCurrentUser
{
    public static void MapGetCurrentUser(this WebApplication app)
    {
        app.MapGet("/api/auth/me", async (HttpContext context, ApplicationDbContext db) =>
        {
            var username = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                          context.User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(username))
            {
                return Results.Unauthorized();
            }

            var user = await db.Users
                .Include(u => u.UserHasRoles)
                .ThenInclude(uhr => uhr.Role)
                .ThenInclude(r => r.RoleHasRights)
                .ThenInclude(rhr => rhr.Right)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(new
            {
                user.Id,
                user.Username,
                user.DisplayName,
                Roles = user.UserHasRoles?.Select(uhr => new
                {
                    uhr.Role.Id,
                    uhr.Role.Name
                }).ToList(),
                UserHasRoles = user.UserHasRoles?.Select(uhr => new
                {
                    uhr.Role.Id,
                    uhr.Role.Name,
                    Role = new
                    {
                        uhr.Role.Id,
                        uhr.Role.Name,
                        RoleHasRights = uhr.Role.RoleHasRights?.Select(rhr => new
                        {
                            rhr.Right.Id,
                            rhr.Right.Name,
                            Right = new
                            {
                                rhr.Right.Id,
                                rhr.Right.Name
                            }
                        }).ToList()
                    }
                }).ToList()
            });
        })
        .RequireAuthorization()
        .WithName("GetCurrentUser")
        .WithOpenApi();
    }
}