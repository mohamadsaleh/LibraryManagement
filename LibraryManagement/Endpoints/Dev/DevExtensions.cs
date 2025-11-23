using LibraryManagement.Endpoints.Dev;
namespace LibraryManagement.Endpoints;

public static class DevExtensions
{
    public static void RegisterDevEndpoints(this WebApplication app)
    {
        app.MapAddEndpointsAsRights();
        app.MapAddRightsToRole();
        app.MapAddRolesToUser();
        app.MapCreateRight();
        app.MapCreateRole();
        app.MapCreateUser();
        app.MapGetProjectEndpointse();
        app.MapGetRightsByRoleId();
        app.MapGetRightsByUserId();
        app.MapGetRightsFromDB();
        app.MapGetRolesByUserId();
        app.MapMigrateConnectedDB();
    }
}

