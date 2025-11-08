using LibraryManagement.Endpoints.Auth;

namespace LibraryManagement.Endpoints;

public static class AuthExtensions
{
    public static void RegisterAuthEndpoints(this WebApplication app)
    {
        app.MapLogin();
        app.MapGetRoles();
        app.MapGetRole();
        app.MapGetRights();
        app.MapCreateRole();
        app.MapUpdateRole();
        app.MapDeleteRole();
        app.MapGetCurrentUser();
    }
}