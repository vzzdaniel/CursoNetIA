using System.Reflection;
using AulaPedidos.Application.Contracts.Persistence;

namespace AulaPedidos.ArchitectureTests;

public class ArchitectureTests
{
    [Fact]
    public void IRepository_ShouldExposeExpectedAsyncMethods()
    {
        var methods = typeof(IRepository<>).GetMethods(BindingFlags.Public | BindingFlags.Instance);

        Assert.Contains(methods, method => method.Name == "FirstOrDefaultAsync");
        Assert.Contains(methods, method => method.Name == "ListAsync");
        Assert.Contains(methods, method => method.Name == "AnyAsync");
        Assert.Contains(methods, method => method.Name == "CountAsync");
        Assert.Contains(methods, method => method.Name == "AddAsync");
        Assert.Contains(methods, method => method.Name == "RemoveAsync");
    }

    [Fact]
    public void IUnitOfWork_ShouldExposeSaveChangesAsync()
    {
        var method = typeof(IUnitOfWork).GetMethod("SaveChangesAsync", BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(method);
    }

    [Fact]
    public void ApplicationAssembly_ShouldNotReferenceEntityFrameworkCore()
    {
        var references = typeof(AulaPedidos.Application.DependencyInjection).Assembly
            .GetReferencedAssemblies()
            .Select(assemblyName => assemblyName.Name)
            .ToList();

        Assert.DoesNotContain("Microsoft.EntityFrameworkCore", references);
    }
}
