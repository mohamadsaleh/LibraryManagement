using LibraryManagement;
using LibraryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LibraryManagement.Endpoints.Auth;

public static class Login
{
    public static void MapLogin(this WebApplication app)
    {
        app.MapPost("/api/auth/login", async ([FromBody] LoginRequest request, ApplicationDbContext db, IConfiguration config) =>
        {
            var user = await db.Users
                .Include(u => u.UserHasRoles)
                .ThenInclude(ur => ur.Role)
                .ThenInclude(r => r.RoleHasRights)
                .ThenInclude(rr => rr.Right)
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Results.Unauthorized();
            }

            // Check if user has any rights
            var hasRights = user.UserHasRoles.Any(ur => ur.Role.RoleHasRights.Any());
            if (!hasRights)
            {
                return Results.Unauthorized();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Username),
                new Claim("sub", user.Username)
            };
            
            // Add role claims
            foreach (var userRole in user.UserHasRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));
                foreach (var roleRight in userRole.Role.RoleHasRights)
                {
                    claims.Add(new Claim("Right", roleRight.Right.Name));
                }
            }
            string x = config["Jwt:Key"];
            if (string.IsNullOrEmpty(config["Jwt:Key"]))
                throw new Exception("JWT Key is not configured.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"] ?? "LibraryManagement",
                audience: config["Jwt:Audience"] ?? "LibraryManagement",
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return Results.Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        })
        .WithName("Login")
        .WithOpenApi();
    }
}

public record LoginRequest(string Username, string Password);