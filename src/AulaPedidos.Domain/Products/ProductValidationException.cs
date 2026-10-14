namespace AulaPedidos.Domain.Products;

public sealed class ProductValidationException(string message) : Exception(message);