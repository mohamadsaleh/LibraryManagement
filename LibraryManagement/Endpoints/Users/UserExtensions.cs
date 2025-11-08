using LibraryManagement.Endpoints.Users;

namespace LibraryManagement.Endpoints;

public static class UserExtensions
{
    public static void RegisterUserEndpoints(this WebApplication app)
    {
        app.MapGetUsers();
        app.MapGetUser();
        app.MapCreateUser();
        app.MapUpdateUser();
        app.MapDeleteUser();
    }
}