using AulaPedidos.Api.Endpoints;
using AulaPedidos.Api.Errors;
using AulaPedidos.Application;
using AulaPedidos.Infrastructure;
using AulaPedidos.Infrastructure.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ProductValidationExceptionHandler>();
builder.Services.AddMediator(options =>
{
    options.Assemblies =
    [
        typeof(AulaPedidos.Application.DependencyInjection).Assembly,
    ];
    options.ServiceLifetime = ServiceLifetime.Scoped;
});
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AulaPedidosDbContext>();
    await dbContext.Database.MigrateAsync(app.Lifetime.ApplicationStopping);
}

app.UseExceptionHandler();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/health");
app.MapProductEndpoints();

app.Run();

public partial class Program;
