using System.Text.Json.Serialization;
using AulaPedidos.Api.Endpoints;
using AulaPedidos.Api.Errors;
using AulaPedidos.Application;
using AulaPedidos.Application.Caching;
using AulaPedidos.Application.Validation;
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
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
});
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<ProductValidationExceptionHandler>();
builder.Services.AddExceptionHandler<UnhandledExceptionHandler>();
builder.Services.AddMediator(options =>
{
    options.Assemblies =
    [
        typeof(AulaPedidos.Application.DependencyInjection).Assembly,
    ];
    options.ServiceLifetime = ServiceLifetime.Scoped;
    options.PipelineBehaviors =
        [
            typeof(ValidationBehavior<,>),
            typeof(CachingBehavior<,>),
            typeof(CacheInvalidationBehavior<,>),
        ];
});
builder.Services.AddMemoryCache();
builder.Services.Configure<ProductCacheOptions>(builder.Configuration.GetSection(ProductCacheOptions.SectionName));
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AulaPedidosDbContext>();
    await dbContext.Database.MigrateAsync(app.Lifetime.ApplicationStopping);
}

app.UseExceptionHandler();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "AulaPedidos API v1");
        options.RoutePrefix = "swagger";
    });
}

app.MapHealthChecks("/health");
app.MapProductEndpoints();
app.MapDiscountEndpoints();

app.Run();

public partial class Program;
