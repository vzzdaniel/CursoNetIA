using Microsoft.EntityFrameworkCore;

namespace AulaPedidos.Infrastructure.Persistence;

public class AulaPedidosDbContext(DbContextOptions options)
    : DbContext(options)
{
}