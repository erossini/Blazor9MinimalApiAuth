using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using MinimalApiAuth.Domain;
using MinimalApiAuth.Persistence;
namespace MinimalApiAuth.Endpoints;

public static class ClientEndpoints
{
    public static void MapClientEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/Client").WithTags(nameof(Domain.Client));

        group.MapGet("/", async (AppDbContext db) =>
        {
            return await db.Clients.ToListAsync();
        })
        .RequireAuthorization()
        .WithName("GetAllClients")
        .WithOpenApi();

        group.MapGet("/{id}", async Task<Results<Ok<Domain.Client>, NotFound>> (long id, AppDbContext db) =>
        {
            return await db.Clients.AsNoTracking()
                .FirstOrDefaultAsync(model => model.Id == id)
                is Domain.Client model
                    ? TypedResults.Ok(model)
                    : TypedResults.NotFound();
        })
        .RequireAuthorization()
        .WithName("GetClientById")
        .WithOpenApi();

        group.MapPut("/{id}", async Task<Results<Ok, NotFound>> (long id, Domain.Client client, AppDbContext db) =>
        {
            var affected = await db.Clients
                .Where(model => model.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(m => m.Id, client.Id)
                    .SetProperty(m => m.FirstName, client.FirstName)
                    .SetProperty(m => m.LastName, client.LastName)
                    .SetProperty(m => m.Email, client.Email)
                    .SetProperty(m => m.Phone, client.Phone)
                    );
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .RequireAuthorization()
        .WithName("UpdateClient")
        .WithOpenApi();

        group.MapPost("/", async (Domain.Client client, AppDbContext db) =>
        {
            db.Clients.Add(client);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/Client/{client.Id}", client);
        })
        .RequireAuthorization()
        .WithName("CreateClient")
        .WithOpenApi();

        group.MapDelete("/{id}", async Task<Results<Ok, NotFound>> (long id, AppDbContext db) =>
        {
            var affected = await db.Clients
                .Where(model => model.Id == id)
                .ExecuteDeleteAsync();
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .RequireAuthorization()
        .WithName("DeleteClient")
        .WithOpenApi();
    }
}
