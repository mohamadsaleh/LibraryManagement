namespace LibraryManagement.Endpoints.Dev;

public static class GetRightsFromDB
{
    public static void MapGetRightsFromDB(this WebApplication app)
    {
        app.MapGet("/api/Dev/GetRightsFromDB", async (ApplicationDbContext db) =>
        {
            var Result=db.Rights.ToList();
            return Results.Ok(Result);
        })
        .WithName("GetRightsFromDBDev")
        .WithOpenApi();
    }
}

