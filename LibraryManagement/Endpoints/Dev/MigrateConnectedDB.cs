using LibraryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Endpoints.Dev;

public static class MigrateConnectedDB
{
    public static void MapMigrateConnectedDB(this WebApplication app)
    {
        app.MapGet("/api/Dev/MigrateConnectedDB", async ([FromQuery] int userId, ApplicationDbContext db) =>
        {
            db.Database.Migrate();

            return Results.Ok("executed db.Database.Migrate();");
        })
        .WithName("MigrateConnectedDB")
        .WithTags("Dev");
    }
}

