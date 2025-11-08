using LibraryManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Endpoints.Dev;

public static class AddEndpointsAsRights
{
    public static void MapAddEndpointsAsRights(this WebApplication app)
    {
        app.MapPost("/api/Dev/AddEndpointsAsRights", async (ApplicationDbContext db) =>
        {
            var endpointDataSource = app.Services.GetRequiredService<EndpointDataSource>();

            var endpoints = endpointDataSource.Endpoints
                .OfType<RouteEndpoint>()
                .Where(endpoint => endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>().Any(authData => authData.Policy == "EndpointName"))
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
            Dictionary<string, List<Right>> Result = new Dictionary<string, List<Right>>();
            List<Right> existedEndpoints = new List<Right>();
            List<Right> addedEndpoints = new List<Right>();
            foreach (var endpoint in endpoints)
            {
                var existingRight = db.Rights.FirstOrDefault(r => r.Name == endpoint.EndpointName);
                var description = $"دسترسی به {endpoint.EndpointName} از طریق مسیر {endpoint.EndpointRoute} با متد {endpoint.Method}";

                if (existingRight == null)
                {
                    Right addedRight = new Models.Right
                    {
                        Name = endpoint.EndpointName!,
                        Description = description,
                        Type = "Endpoint"
                    };
                    db.Rights.Add(addedRight);
                    addedEndpoints.Add(addedRight);
                }
                else
                {
                    existedEndpoints.Add(existingRight);
                }
            }
            db.SaveChanges();
            Result.Add("addedEndpoints", addedEndpoints);
            Result.Add("existedEndpoints", existedEndpoints);
            return Results.Ok(Result);
        })
        .WithName("AddEndpointsAsRightsDev")
        .WithOpenApi();
    }
}

