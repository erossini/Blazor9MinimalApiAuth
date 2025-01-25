using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using MinimalApiAuth.Domain;
using MinimalApiAuth.Persistence;
namespace MinimalApiAuth.Endpoints;

public static class ClientAddressEndpoints
{
    public static void MapClientAddressEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/ClientAddress").WithTags(nameof(ClientAddress));

        group.MapGet("/", async (AppDbContext db) =>
        {
            return await db.ClientAddresses.ToListAsync();
        })
        .RequireAuthorization()
        .WithName("GetAllClientAddresses")
        .WithOpenApi();

        group.MapGet("/{id}", async Task<Results<Ok<ClientAddress>, NotFound>> (long id, AppDbContext db) =>
        {
            return await db.ClientAddresses.AsNoTracking()
                .FirstOrDefaultAsync(model => model.Id == id)
                is ClientAddress model
                    ? TypedResults.Ok(model)
                    : TypedResults.NotFound();
        })
        .RequireAuthorization()
        .WithName("GetClientAddressById")
        .WithOpenApi();

        group.MapGet("/client/{id}", async Task<Results<Ok<ClientAddress>, NotFound>> (long id, AppDbContext db) =>
        {
            return await db.ClientAddresses.AsNoTracking()
                .FirstOrDefaultAsync(model => model.ClientId == id)
                is ClientAddress model
                    ? TypedResults.Ok(model)
                    : TypedResults.NotFound();
        })
        .RequireAuthorization()
        .WithName("GetClientAddressByClientId")
        .WithOpenApi();

        group.MapPut("/{id}", async Task<Results<Ok, NotFound>> (long id, ClientAddress clientAddress, AppDbContext db) =>
        {
            var affected = await db.ClientAddresses
                .Where(model => model.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(m => m.Id, clientAddress.Id)
                    .SetProperty(m => m.Address1, clientAddress.Address1)
                    .SetProperty(m => m.Number, clientAddress.Number)
                    .SetProperty(m => m.Address2, clientAddress.Address2)
                    .SetProperty(m => m.City, clientAddress.City)
                    .SetProperty(m => m.Country, clientAddress.Country)
                    .SetProperty(m => m.State, clientAddress.State)
                    .SetProperty(m => m.Postcode, clientAddress.Postcode)
                    );
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .RequireAuthorization()
        .WithName("UpdateClientAddress")
        .WithOpenApi();

        group.MapPost("/", async (ClientAddress clientAddress, AppDbContext db) =>
        {
            db.ClientAddresses.Add(clientAddress);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/ClientAddress/{clientAddress.Id}", clientAddress);
        })
        .RequireAuthorization()
        .WithName("CreateClientAddress")
        .WithOpenApi();

        group.MapDelete("/{id}", async Task<Results<Ok, NotFound>> (long id, AppDbContext db) =>
        {
            var affected = await db.ClientAddresses
                .Where(model => model.Id == id)
                .ExecuteDeleteAsync();
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .RequireAuthorization()
        .WithName("DeleteClientAddress")
        .WithOpenApi();
    }
}
