using LibraryManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Endpoints.Auth;

public static class GetRights
{
    public static void MapGetRights(this WebApplication app)
    {
        app.MapGet("/api/rights", async (ApplicationDbContext db) =>
        {
            var rights = await db.Rights.ToListAsync();
            return Results.Ok(rights);
        })
        .RequireEndpointName()
        .WithName("GetRights")
        .WithOpenApi();
    }
}
