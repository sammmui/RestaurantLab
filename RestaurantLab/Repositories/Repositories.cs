using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RestaurantLab.Data;
using RestaurantLab.Models;

namespace RestaurantLab.Repositories;

public class MenuItemRepository : IMenuItemRepository
{
    private readonly RestaurantDbContext _db;
    public MenuItemRepository(RestaurantDbContext db) => _db = db;

    public Task<List<MenuItemDto>> GetAllAsync() =>
        _db.MenuItems.AsNoTracking().OrderBy(m => m.CategoryName).ThenBy(m => m.Name).ToListAsync();

    public Task SoftDeleteAsync(int menuItemId, int userId) =>
        _db.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC usp_SoftDeleteMenuItem @MenuItemId = {menuItemId}, @ModifiedByUserId = {userId}");
}

public class EmployeeRepository : IEmployeeRepository
{
    private readonly RestaurantDbContext _db;
    public EmployeeRepository(RestaurantDbContext db) => _db = db;

    public Task<List<EmployeeDto>> GetAllAsync() =>
        _db.Employees.AsNoTracking().ToListAsync();

    public Task SoftDeleteAsync(int employeeId, int userId) =>
        _db.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC usp_SoftDeleteEmployee @EmployeeId = {employeeId}, @ModifiedByUserId = {userId}");

    public Task RestoreAsync(int employeeId, int userId) =>
        _db.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC usp_RestoreEmployee @EmployeeId = {employeeId}, @ModifiedByUserId = {userId}");
}

public class OrderRepository : IOrderRepository
{
    private readonly RestaurantDbContext _db;
    public OrderRepository(RestaurantDbContext db) => _db = db;

    public async Task<int> CreateAsync(int employeeId, int tableId, int? customerId, int userId)
    {
        var newId = new SqlParameter("@NewOrderId", SqlDbType.Int) { Direction = ParameterDirection.Output };

        await _db.Database.ExecuteSqlRawAsync(
            "EXEC usp_CreateOrder @EmployeeId, @TableId, @CustomerId, @UserId, @NewOrderId OUTPUT",
            new SqlParameter("@EmployeeId", employeeId),
            new SqlParameter("@TableId", tableId),
            new SqlParameter("@CustomerId", (object?)customerId ?? DBNull.Value),
            new SqlParameter("@UserId", userId),
            newId);

        return (int)newId.Value;
    }

    public Task AddItemAsync(int orderId, int menuItemId, int quantity, int userId) =>
        _db.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC usp_AddOrderItem @OrderId = {orderId}, @MenuItemId = {menuItemId}, @Quantity = {quantity}, @UserId = {userId}");

    public Task<List<OrderLineDto>> GetLinesAsync(int orderId) =>
        _db.OrderLines.AsNoTracking().Where(l => l.OrderId == orderId).ToListAsync();
}
