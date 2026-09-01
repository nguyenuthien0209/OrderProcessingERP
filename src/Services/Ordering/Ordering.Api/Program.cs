using Common.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Ordering.Application;
using Ordering.Infrastructure;
using Ordering.Infrastructure.Persistence;
using Serilog;

const string ApiScope = "ordering.api";

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            ClientCredentials = new OpenApiOAuthFlow
            {
                TokenUrl = new Uri($"{builder.Configuration["Identity:Authority"]}/connect/token"),
                Scopes = new Dictionary<string, string> { [ApiScope] = "Ordering API" }
            }
        }
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" } }] = [ApiScope]
    });
});

builder.Services.AddJwtBearerAuthentication(builder.Configuration, ApiScope);
builder.Services.AddOrderingApplication();
builder.Services.AddOrderingInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.OAuthClientId("swagger"));
}

app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Dev convenience only: in a real deployment, migrations run as a separate pipeline step, not on every boot.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
    dbContext.Database.Migrate();
}

app.Run();
