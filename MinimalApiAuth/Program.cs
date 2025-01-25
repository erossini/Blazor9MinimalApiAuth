using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MinimalApiAuth.Client.Pages;
using MinimalApiAuth.Components;
using MinimalApiAuth.Components.Account;
using MinimalApiAuth.Data;
using MinimalApiAuth.Persistence;
using Serilog;
using MinimalApiAuth.Endpoints;

var builder = WebApplication.CreateBuilder(args);

#region Serilog

Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs/net9demo-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

builder.Services.AddSerilog();

#endregion Serilog

builder.Services.AddAppPersistenceServices(builder.Configuration);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
};

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(MinimalApiAuth.Client._Imports).Assembly);

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

#region Database

// Create the database if it does not exist
using (var scope = app.Services.CreateScope())
{
    var service = scope.ServiceProvider;

    // Apply changes for the Individual Identity
    var context = service.GetService<ApplicationDbContext>();
    try
    {
        context?.Database?.EnsureCreated();
        context?.Database?.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
    }
}

using (var scope = app.Services.CreateScope())
{
    var service = scope.ServiceProvider;

    // Apply changes for the application domain
    var appContext = service.GetService<AppDbContext>();
    try
    {
        appContext?.Database?.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
    }
}

app.MapClientEndpoints();
app.MapClientAddressEndpoints();

#endregion

app.Run();
