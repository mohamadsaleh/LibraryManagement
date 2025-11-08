using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authorization;

namespace LibraryManagement.Endpoints;

public static class AuthorizationExtensions
{
    public static RouteHandlerBuilder RequireAccessRight(this RouteHandlerBuilder builder)
    {
        return builder.RequireAuthorization(new AuthorizeAttribute { Policy = "AccessRight" });
    }

    public static RouteHandlerBuilder RequireEndpointName(this RouteHandlerBuilder builder)
    {
        return builder.RequireAuthorization(new AuthorizeAttribute { Policy = "EndpointName" });
    }
}