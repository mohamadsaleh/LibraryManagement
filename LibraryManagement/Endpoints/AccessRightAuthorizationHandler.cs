using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LibraryManagement.Endpoints;

public class AccessRightRequirement : IAuthorizationRequirement
{
}

public class EndpointNameRequirement : IAuthorizationRequirement
{
}

public class AccessRightAuthorizationHandler : AuthorizationHandler<AccessRightRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessRightRequirement requirement)
    {
        var endpoint = context.Resource as Endpoint;
        if (endpoint == null)
        {
            // Fallback: try to get from HttpContext
            var httpContext = context.Resource as HttpContext;
            if (httpContext != null)
            {
                endpoint = httpContext.GetEndpoint();
            }
        }

        if (endpoint == null)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        // Check AccessRightAttribute
        var accessRightAttribute = endpoint.Metadata.GetMetadata<AccessRightAttribute>();
        if (accessRightAttribute != null)
        {
            if (context.User.HasClaim("Right", accessRightAttribute.Name))
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }
        else
        {
            // If no AccessRightAttribute, succeed (allow access)
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

public class EndpointNameAuthorizationHandler : AuthorizationHandler<EndpointNameRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, EndpointNameRequirement requirement)
    {
        var endpoint = context.Resource as Endpoint;
        if (endpoint == null)
        {
            // Fallback: try to get from HttpContext
            var httpContext = context.Resource as HttpContext;
            if (httpContext != null)
            {
                endpoint = httpContext.GetEndpoint();
            }
        }

        if (endpoint == null)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        // Check endpoint name as right
        var endpointName = endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName;
        Console.WriteLine($"EndpointName Handler - EndpointName: {endpointName}");
        Console.WriteLine($"User claims: {string.Join(", ", context.User.Claims.Select(c => $"{c.Type}:{c.Value}"))}");

        if (!string.IsNullOrEmpty(endpointName))
        {
            if (context.User.HasClaim("Right", endpointName))
            {
                Console.WriteLine($"User has right '{endpointName}' - Access granted");
                context.Succeed(requirement);
            }
            else
            {
                Console.WriteLine($"User does NOT have right '{endpointName}' - Access denied");
                context.Fail();
            }
        }
        else
        {
            // If no endpoint name, allow access
            Console.WriteLine("No endpoint name found - Access granted");
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}