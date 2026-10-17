using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RestaurantLab.Models;
using RestaurantLab.UoW;

namespace RestaurantLab.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    public OrdersController(IUnitOfWork uow) => _uow = uow;

    // POST api/orders  (замовлення + позиції в одній транзакції)
    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderRequest req)
    {
        if (req.Items.Count == 0)
            return BadRequest(new { error = "Order must contain at least one item" });

        await _uow.BeginAsync();
        try
        {
            int orderId = await _uow.Orders.CreateAsync(
                req.EmployeeId, req.TableId, req.CustomerId, req.UserId);

            foreach (var item in req.Items)
                await _uow.Orders.AddItemAsync(orderId, item.MenuItemId, item.Quantity, req.UserId);

            await _uow.CommitAsync();

            var lines = await _uow.Orders.GetLinesAsync(orderId);
            return CreatedAtAction(nameof(Get), new { id = orderId }, lines);
        }
        catch (SqlException ex)
        {
            await _uow.RollbackAsync();
            return BadRequest(new { error = ex.Message });
        }
    }

    // GET api/orders/7
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var lines = await _uow.Orders.GetLinesAsync(id);
        return lines.Count == 0 ? NotFound() : Ok(lines);
    }
}