using LibraryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Endpoints.Dev;

public static class GetRightsByUserId
{
    public static void MapGetRightsByUserId(this WebApplication app)
    {
        app.MapGet("/api/Dev/GetRightsByUserId", async ([FromQuery]int userId, ApplicationDbContext db) =>
        {
            // ابتدا بررسی می‌کنیم که آیا کاربر وجود دارد یا خیر
            var userExists = await db.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                return Results.NotFound(new { message = $"User with ID {userId} not found." });
            }

            // کوئری برای استخراج دسترسی‌ها از طریق جداول واسط
            var rights = await db.UserHasRoles
                .Where(uhr => uhr.UserId == userId)         // 1. فیلتر بر اساس کاربر در جدول UserHasRole
                .Select(uhr => uhr.Role)                    // 2. انتخاب نقش‌های مرتبط
                .SelectMany(r => r.RoleHasRights)           // 3. رفتن به جدول واسط RoleHasRight
                .Select(rhr => rhr.Right)                   // 4. انتخاب دسترسی‌های نهایی
                .Distinct()                                 // 5. حذف دسترسی‌های تکراری
                .OrderBy(right => right.Name)
                .AsNoTracking()
                .ToListAsync();

            if (!rights.Any())
            {
                return Results.Ok(new { message = $"User with ID {userId} has no rights.", rights = new List<Right>() });
            }

            return Results.Ok(rights);
        })
        .WithName("GetRightsByUserIdDev")
        .WithTags("Dev")
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Get all unique rights for a specific user.",
            Description = "This endpoint retrieves a consolidated list of all unique rights assigned to a user through their roles."
        });
    }
}