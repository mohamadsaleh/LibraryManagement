
using LibraryManagement;
using LibraryManagement.Endpoints;
using LibraryManagement.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]
            ?? throw new Exception("JWT Key is not configured.")))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AccessRight", policy =>
        policy.Requirements.Add(new AccessRightRequirement()));

    options.AddPolicy("EndpointName", policy =>
        policy.Requirements.Add(new EndpointNameRequirement()));
});

builder.Services.AddSingleton<IAuthorizationHandler, AccessRightAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, EndpointNameAuthorizationHandler>();

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

// Register endpoints
app.RegisterAuthEndpoints();
app.RegisterStudentEndpoints();
app.RegisterBookEndpoints();
app.RegisterLoanEndpoints();
app.RegisterUserEndpoints();
app.RegisterDevEndpoints();

app.UseDeveloperExceptionPage();

app.Run();
//var task = app.RunAsync();

//// Seed data after the application is built and endpoints are mapped
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    var dbContext = services.GetRequiredService<ApplicationDbContext>();

//    // No need to pass 'app' again, it's already available via 'this' extension method
//    app.SeedEndpointsAsRights(dbContext);
//    app.SeedFullAdminUserWithAllRights(dbContext);
//}

//await task;



namespace LibraryManagement
{
    public partial class Program;
}
