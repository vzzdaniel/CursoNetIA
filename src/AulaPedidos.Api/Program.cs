using AulaPedidos.Application;
using AulaPedidos.Infrastructure;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/health");

app.Run();

public partial class Program;
