using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

#region HttpClient

builder.Services
    .AddScoped(sp => sp
        .GetRequiredService<IHttpClientFactory>()
        .CreateClient("ServerAPI"))
        .AddHttpClient("ServerAPI", (provider, client) =>
        {
            client.BaseAddress = new Uri("https://localhost:7130");
        });

#endregion

await builder.Build().RunAsync();
