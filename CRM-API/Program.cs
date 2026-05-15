
using CRM_API.Data.Models;
using Microsoft.Azure.Cosmos;

namespace CRM_API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Cosmos config
            var endpoint = "https://localhost:8081";

            var cosmosClient = new CosmosClient(endpoint, builder.Configuration["CosmosDb:Key"]);
            var database = await cosmosClient.CreateDatabaseIfNotExistsAsync(builder.Configuration["CosmosDb:DatabaseName"]);
            var container = await database.Database.CreateContainerIfNotExistsAsync(builder.Configuration["CosmosDb:ContainerName"], "/id");

            // Add services to the container.
            builder.Services.AddAuthorization();
            builder.Services.AddSingleton<Container>(container);

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseAuthorization();

            app.MapGet("/", () => "API is running");

            app.MapPost("/customers", async (Customer customer, Container container) =>
            {
                customer.Id = Guid.NewGuid().ToString();

                var response = await container.CreateItemAsync(customer, new PartitionKey(customer.Id));

                return Results.Ok(response.Resource);
            });

            app.MapGet("/customers/search/by-name", async (string name, Container container) =>
            {
                var query = new QueryDefinition(
                    "SELECT * FROM c WHERE CONTAINS(c.Name, @name)")
                .WithParameter("@name", name);

                var iterator = container.GetItemQueryIterator<Customer>(query);

                var results = new List<Customer>();

                while(iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    results.AddRange(response);
                }

                return Results.Ok(results);
            });

            app.MapGet("customers/search/by-salesperson", 
                async (string salespersonName, Container container) =>
            {
                var query = new QueryDefinition(
                    "SELECT * FROM c WHERE CONTAINS(c.ResponsibleSalesPerson.Name, @salespersonName)")
                .WithParameter("@salespersonName", salespersonName);

                var iterator = container.GetItemQueryIterator<Customer>(query);

                var results = new List<Customer>();

                while(iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    results.AddRange(response);
                }

                return Results.Ok(results);
            });

            app.MapPut("/customers/{id}", async (string id, Customer updatedCustomer, Container container) =>
            {
                updatedCustomer.Id = id;

                var response = await container.UpsertItemAsync(updatedCustomer, new PartitionKey(id));

                return Results.Ok(response.Resource);
            });

            app.MapDelete("/customers/{id}", async (string id, Container container) =>
            {
                await container.DeleteItemAsync<Customer>(id, new PartitionKey(id));

                return Results.Ok($"Customer {id} deleted");
            });

            app.MapGet("/customers", async (Container container) =>
            {
                var query = new QueryDefinition(
                    "SELECT * FROM c");

                var iterator = container.GetItemQueryIterator<Customer>(query);

                var results = new List<Customer>();

                while(iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    results.AddRange(response);
                }

                return Results.Ok(results);
            });

            app.Run();
        }
    }
}
