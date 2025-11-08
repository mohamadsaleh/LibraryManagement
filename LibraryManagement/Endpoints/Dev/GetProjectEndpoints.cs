using LibraryManagement;
using LibraryManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Endpoints.Dev;

public static class GetProjectEndpoints
{
        public static void MapGetProjectEndpointse(this WebApplication app)
        {
            app.MapGet("/api/Dev/GetProjectEndpoints",  async (ApplicationDbContext db,
                [FromQuery] bool hasEndpointNameRequire = true) =>
            {
                var endpointDataSource = app.Services.GetRequiredService<EndpointDataSource>();

                var endpoints = endpointDataSource.Endpoints
                    .OfType<RouteEndpoint>()
                    .Where(endpoint => hasEndpointNameRequire ? 
                        endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>().Any(authData => authData.Policy == "EndpointName")
                        : true)
                    .Select(endpoint =>
                    {
                        var endpointNameMetadata = endpoint.Metadata.GetMetadata<IEndpointNameMetadata>();
                        var httpMethodMetadata = endpoint.Metadata.GetMetadata<HttpMethodMetadata>();
                        var authEndpointNameRequirementMetadata = endpoint.Metadata.GetMetadata<IAuthorizeData>();
                        
                        var method = httpMethodMetadata != null && httpMethodMetadata.HttpMethods.Any()
                            ? string.Join(", ", httpMethodMetadata.HttpMethods)
                            : "ANY";

                        return new
                        {
                            EndpointName = endpointNameMetadata?.EndpointName,
                            EndpointRoute = endpoint.RoutePattern.RawText,
                            Method = method,
                            AuthorizeMeta = authEndpointNameRequirementMetadata
                        };
                    })
                    .Where(x => x.EndpointName != null && x.EndpointRoute != null)
                    .Distinct()
                    .OrderBy(x => x.EndpointName)
                    .ToList();

                return Results.Ok(endpoints);
            })
            .WithName("GetProjectEndpoints")
            .WithOpenApi();
        }
}