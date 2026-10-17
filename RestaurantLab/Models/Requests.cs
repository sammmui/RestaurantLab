namespace RestaurantLab.Models;

public class OrderItemRequest
{
    public int MenuItemId { get; set; }
    public int Quantity { get; set; }
}

public class CreateOrderRequest
{
    public int EmployeeId { get; set; }
    public int TableId { get; set; }
    public int? CustomerId { get; set; }
    public int UserId { get; set; }
    public List<OrderItemRequest> Items { get; set; } = new();
}