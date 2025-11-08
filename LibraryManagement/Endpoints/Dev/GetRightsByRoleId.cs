
using LibraryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Endpoints.Dev;

public static class GetRightsByRoleId
{
    public static void MapGetRightsByRoleId(this WebApplication app)
    {
        app.MapGet("/api/Dev/GetRightsByRoleId", async ([FromQuery]int roleId, ApplicationDbContext db) =>
        {
            // ابتدا بررسی می‌کنیم که آیا نقش مورد نظر وجود دارد یا خیر
            var roleExists = await db.Roles.AnyAsync(r => r.Id == roleId);
            if (!roleExists)
            {
                return Results.NotFound(new { message = $"Role with ID {roleId} not found." });
            }

            // کوئری برای استخراج دسترسی‌ها از طریق جدول واسط RoleHasRight
            var rights = await db.RoleHasRights
                .Where(rhr => rhr.RoleId == roleId) // 1. فیلتر بر اساس نقش در جدول RoleHasRight
                .Select(rhr => rhr.Right)          // 2. انتخاب دسترسی‌های مرتبط
                .OrderBy(r => r.Name)
                .AsNoTracking()
                .ToListAsync();

            // Return the list of rights for that role
            return Results.Ok(rights);
        })
        .WithName("GetRightsByRoleIdDev")
        .WithTags("Dev")
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Get all rights associated with a specific role.",
            Description = "This endpoint retrieves a list of rights assigned to a role by its ID."
        });
    }
}
