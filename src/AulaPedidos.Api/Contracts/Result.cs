using System.Text.Json.Serialization;

namespace AulaPedidos.Api.Contracts;

public sealed record Result<TData>
{
    public bool IsSuccess { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string? Message { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public TData? Data { get; init; }

    public static Result<TData> Success(TData? data, string? message = null) => new()
    {
        IsSuccess = true,
        Message = message,
        Data = data,
    };

    public static Result<TData> Failure(string? message, TData? data = default) => new()
    {
        IsSuccess = false,
        Message = message,
        Data = data,
    };
}