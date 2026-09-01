using Duende.IdentityServer.EntityFramework.DbContexts;
using Identity.Api.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

var connectionString = builder.Configuration.GetConnectionString("IdentityDb");
var migrationsAssembly = typeof(Program).Assembly.GetName().Name;

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services
    .AddIdentityServer(options => options.EmitStaticAudienceClaim = true)
    .AddConfigurationStore(options =>
        options.ConfigureDbContext = b => b.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly)))
    .AddOperationalStore(options =>
        options.ConfigureDbContext = b => b.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly)))
    .AddAspNetIdentity<ApplicationUser>()
    // Dev convenience only: persists a temp signing key to disk. A real deployment needs a real certificate.
    .AddDeveloperSigningCredential();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseIdentityServer();

// Dev convenience only: in a real deployment, migrations run as a separate pipeline step, not on every boot.
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();
    scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>().Database.Migrate();
    scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

    await SeedData.EnsureSeedDataAsync(scope.ServiceProvider);
}

app.Run();
