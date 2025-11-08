
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;

// ...بقیه کدهای Program.cs

namespace LibraryManagement.Utilities
{
    public static class SeedData
    {
        public static void SeedFullAdminUserWithAllRights(this WebApplication app,
            ApplicationDbContext db)
        {
            var existingUser = db.Users.FirstOrDefault(r => r.Username == "FullAdmin");
            if (existingUser == null)
            {
                var user = new Models.User
                {
                    DisplayName = "Full Admin",
                    Username = "FullAdmin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("fulladmin1369")
                };
                db.Users.Add(user);
                db.SaveChanges();
                existingUser = user;
            }
            var existingRole = db.Roles.FirstOrDefault(r => r.Name == "FullAccess");
            if (existingRole == null)
            {
                var role = new Models.Role
                {
                    Name = "FullAccess"
                };
                db.Roles.Add(role);
                db.SaveChanges();
                existingRole = role;
            }
            var rights = db.Rights.ToList();
            foreach (var right in rights)
            {
                var existingRoleRight = db.RoleHasRights
                    .FirstOrDefault(rr => rr.RoleId == existingRole.Id && rr.RightId == right.Id);
                if (existingRoleRight == null)
                {
                    db.RoleHasRights.Add(new Models.RoleHasRight
                    {
                        RoleId = existingRole.Id,
                        RightId = right.Id
                    });
                }
            }
            var existingUserRole = db.UserHasRoles
                .FirstOrDefault(ur => ur.UserId == existingUser.Id && ur.RoleId == existingRole.Id);
            if (existingUserRole == null)
            {
                db.UserHasRoles.Add(new Models.UserHasRole
                {
                    UserId = existingUser.Id,
                    RoleId = existingRole.Id
                });
            }
            db.SaveChanges();
        }
        public static void SeedEndpointsAsRights(this WebApplication app, ApplicationDbContext db)
        {
            var endpointDataSource = app.Services.GetRequiredService<EndpointDataSource>();

            var endpoints = endpointDataSource.Endpoints
                .OfType<RouteEndpoint>()
                .Select(endpoint =>
                {
                    var endpointNameMetadata = endpoint.Metadata.GetMetadata<IEndpointNameMetadata>();
                    var httpMethodMetadata = endpoint.Metadata.GetMetadata<HttpMethodMetadata>();

                    var method = httpMethodMetadata != null && httpMethodMetadata.HttpMethods.Any()
                        ? string.Join(", ", httpMethodMetadata.HttpMethods)
                        : "ANY";

                    return new
                    {
                        EndpointName = endpointNameMetadata?.EndpointName,
                        EndpointRoute = endpoint.RoutePattern.RawText,
                        Method = method
                    };
                })
                .Where(x => x.EndpointName != null && x.EndpointRoute != null)
                .Distinct()
                .OrderBy(x => x.EndpointName)
                .ToList();

            foreach (var endpoint in endpoints)
            {
                var existingRight = db.Rights.FirstOrDefault(r => r.Name == endpoint.EndpointName);
                var description = $"دسترسی به {endpoint.EndpointName} از طریق مسیر {endpoint.EndpointRoute} با متد {endpoint.Method}";

                if (existingRight == null)
                {
                    db.Rights.Add(new Models.Right
                    {
                        Name = endpoint.EndpointName!,
                        Description = description,
                        Type = "Endpoint"
                    });
                }
                else
                {
                    existingRight.Description = description;
                }
            }
            db.SaveChanges();
        }
    }
}
