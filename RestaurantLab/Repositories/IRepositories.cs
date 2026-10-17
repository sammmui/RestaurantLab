using RestaurantLab.Models;

namespace RestaurantLab.Repositories;

public interface IMenuItemRepository
{
    Task<List<MenuItemDto>> GetAllAsync();
    Task SoftDeleteAsync(int menuItemId, int userId);
}

public interface IEmployeeRepository
{
    Task<List<EmployeeDto>> GetAllAsync();
    Task SoftDeleteAsync(int employeeId, int userId);
    Task RestoreAsync(int employeeId, int userId);
}

public interface IOrderRepository
{
    Task<int> CreateAsync(int employeeId, int tableId, int? customerId, int userId);
    Task AddItemAsync(int orderId, int menuItemId, int quantity, int userId);
    Task<List<OrderLineDto>> GetLinesAsync(int orderId);
}