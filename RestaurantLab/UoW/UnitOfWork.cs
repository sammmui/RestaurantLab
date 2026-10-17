using Microsoft.EntityFrameworkCore.Storage;
using RestaurantLab.Data;
using RestaurantLab.Repositories;

namespace RestaurantLab.UoW;

public interface IUnitOfWork : IAsyncDisposable
{
    IOrderRepository Orders { get; }
    IMenuItemRepository MenuItems { get; }
    IEmployeeRepository Employees { get; }

    Task BeginAsync();
    Task CommitAsync();
    Task RollbackAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly RestaurantDbContext _db;
    private IDbContextTransaction? _tx;

    public UnitOfWork(RestaurantDbContext db)
    {
        _db = db;
        Orders = new OrderRepository(db);
        MenuItems = new MenuItemRepository(db);
        Employees = new EmployeeRepository(db);
    }

    public IOrderRepository Orders { get; }
    public IMenuItemRepository MenuItems { get; }
    public IEmployeeRepository Employees { get; }

    public async Task BeginAsync() => _tx = await _db.Database.BeginTransactionAsync();

    public async Task CommitAsync()
    {
        if (_tx is null) return;
        await _tx.CommitAsync();
        await _tx.DisposeAsync();
        _tx = null;
    }

    public async Task RollbackAsync()
    {
        if (_tx is null) return;
        await _tx.RollbackAsync();
        await _tx.DisposeAsync();
        _tx = null;
    }

    public async ValueTask DisposeAsync()
    {
        if (_tx is not null) await _tx.DisposeAsync(); // без Commit це автоматичний rollback
        await _db.DisposeAsync();
    }
}