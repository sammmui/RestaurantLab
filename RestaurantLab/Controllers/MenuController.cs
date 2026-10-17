using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RestaurantLab.UoW;

namespace RestaurantLab.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    public MenuController(IUnitOfWork uow) => _uow = uow;

    // GET api/menu
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _uow.MenuItems.GetAllAsync());

    // DELETE api/menu/3?userId=1
    [HttpDelete("{id}")]
    public async Task<IActionResult> SoftDelete(int id, [FromQuery] int userId)
    {
        try
        {
            await _uow.MenuItems.SoftDeleteAsync(id, userId);
            return NoContent();
        }
        catch (SqlException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}