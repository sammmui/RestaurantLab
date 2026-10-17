using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RestaurantLab.UoW;

namespace RestaurantLab.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    public EmployeesController(IUnitOfWork uow) => _uow = uow;

    // GET api/employees
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _uow.Employees.GetAllAsync());

    // DELETE api/employees/1?userId=1
    [HttpDelete("{id}")]
    public async Task<IActionResult> SoftDelete(int id, [FromQuery] int userId)
    {
        try
        {
            await _uow.Employees.SoftDeleteAsync(id, userId);
            return NoContent();
        }
        catch (SqlException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    // POST api/employees/1/restore?userId=1
    [HttpPost("{id}/restore")]
    public async Task<IActionResult> Restore(int id, [FromQuery] int userId)
    {
        try
        {
            await _uow.Employees.RestoreAsync(id, userId);
            return NoContent();
        }
        catch (SqlException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}