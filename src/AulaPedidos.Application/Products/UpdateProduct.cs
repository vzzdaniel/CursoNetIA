using AulaPedidos.Application.Contracts.Persistence;
using AulaPedidos.Domain.Products;
using Mediator;

namespace AulaPedidos.Application.Products;

public sealed record UpdateProduct(Guid Id, decimal Price, string? Description, string? AlternateDescription) : ICommand<ProductDto?>;

public sealed class UpdateProductHandler(IRepository<Product> repository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateProduct, ProductDto?>
{
    public async ValueTask<ProductDto?> Handle(UpdateProduct command, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var product = await repository.FirstOrDefaultAsync(product => product.Id == command.Id, cancellationToken);
        if (product is null)
        {
            return null;
        }

        product.Update(command.Price, command.Description, command.AlternateDescription);
        _ = await unitOfWork.SaveChangesAsync(cancellationToken);
        return ProductDto.FromProduct(product);
    }
}