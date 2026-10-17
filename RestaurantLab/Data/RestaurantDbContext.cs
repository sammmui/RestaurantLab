using Microsoft.EntityFrameworkCore;
using RestaurantLab.Models;

namespace RestaurantLab.Data;

public class RestaurantDbContext : DbContext
{
    public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options) : base(options) { }

    public DbSet<MenuItemDto> MenuItems => Set<MenuItemDto>();
    public DbSet<EmployeeDto> Employees => Set<EmployeeDto>();
    public DbSet<OrderLineDto> OrderLines => Set<OrderLineDto>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<MenuItemDto>().HasNoKey().ToView("vw_ActiveMenuItems");
        mb.Entity<EmployeeDto>().HasNoKey().ToView("vw_ActiveEmployees");
        mb.Entity<OrderLineDto>().HasNoKey().ToView("vw_OrderLines");
    }
}