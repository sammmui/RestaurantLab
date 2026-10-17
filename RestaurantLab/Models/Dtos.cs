namespace RestaurantLab.Models;

public class MenuItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public string CategoryName { get; set; } = "";
}

public class EmployeeDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string RoleName { get; set; } = "";
    public string? Phone { get; set; }
}

public class OrderLineDto
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public string MenuItemName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}