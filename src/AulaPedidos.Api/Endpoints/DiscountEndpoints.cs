namespace AulaPedidos.Api.Endpoints;

public static class DiscountEndpoints
{
    public static IEndpointRouteBuilder MapDiscountEndpoints(this IEndpointRouteBuilder app)
    {
        _ = app.MapGet("/mock/discount", () => Results.Ok(new { Percentage = Random.Shared.Next(10, 31) }));
        return app;
    }
}
