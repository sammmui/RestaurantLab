using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantLab.Data;
using RestaurantLab.Models;
using RestaurantLab.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantLab.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NoSqlOrdersController : ControllerBase
    {
        private readonly MongoOrderService _mongoService = new MongoOrderService();
        private readonly RestaurantDbContext _sqlContext;

        public NoSqlOrdersController(RestaurantDbContext sqlContext)
        {
            _sqlContext = sqlContext;
        }

        [HttpPost("test-insert")]
        public async Task<IActionResult> TestInsert()
        {
            var doc = new OrderHistoryDocument
            {
                OriginalOrderId = 1054,
                EmployeeId = 3,
                TableId = 5,
                OrderDate = DateTime.UtcNow,
                TotalAmount = 1250.00m,
                Status = "paid",
                Items = new List<OrderItemEmbed>
                {
                    new OrderItemEmbed { MenuItemId = 42, Name = "Стейк Рібай", Quantity = 2, UnitPrice = 500.00m },
                    new OrderItemEmbed { MenuItemId = 12, Name = "Кава Еспресо", Quantity = 5, UnitPrice = 50.00m }
                },
                Payment = new PaymentEmbed
                {
                    MethodId = 1,
                    MethodName = "Card",
                    AmountPaid = 1250.00m,
                    PaymentDate = DateTime.UtcNow
                }
            };

            await _mongoService.InsertOrderAsync(doc);
            return Ok(new { message = "Успішно збережено в MongoDB!", mongoId = doc.Id });
        }

        [HttpPost("migrate/{orderId}")]
        public async Task<IActionResult> MigrateOrder(int orderId)
        {
            var orderLines = await _sqlContext.OrderLines
                .Where(o => o.OrderId == orderId)
                .ToListAsync();

            if (!orderLines.Any())
            {
                return NotFound(new { error = $"Замовлення №{orderId} не знайдено в MS SQL" });
            }

            var firstLine = orderLines.First();

            var doc = new OrderHistoryDocument
            {
                OriginalOrderId = firstLine.OrderId,
                EmployeeId = 1,
                TableId = 1,
                OrderDate = firstLine.OrderDate,
                TotalAmount = orderLines.Sum(l => l.LineTotal),
                Status = "archived",
                Items = orderLines.Select(l => new OrderItemEmbed
                {
                    MenuItemId = 1,
                    Name = l.MenuItemName ?? "Невідомо",
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice
                }).ToList(),
                Payment = new PaymentEmbed
                {
                    MethodId = 1,
                    MethodName = "Card",
                    AmountPaid = orderLines.Sum(l => l.LineTotal),
                    PaymentDate = firstLine.OrderDate
                }
            };

            await _mongoService.InsertOrderAsync(doc);

            return Ok(new
            {
                message = $"Замовлення №{orderId} успішно перенесено з MS SQL в MongoDB!",
                mongoId = doc.Id
            });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var order = await _mongoService.GetOrderByIdAsync(id);
            if (order == null) return NotFound(new { error = "Документ не знайдено в MongoDB" });
            return Ok(order);
        }
        [HttpGet("benchmark/{orderId}")]
        public async Task<IActionResult> Benchmark(int orderId, [FromQuery] int iterations = 100)
        {
            var sw = new System.Diagnostics.Stopwatch();

            sw.Start();
            for (int i = 0; i < iterations; i++)
            {
                var sqlLines = await _sqlContext.OrderLines
                    .Where(o => o.OrderId == orderId)
                    .ToListAsync();
            }
            sw.Stop();
            long sqlTime = sw.ElapsedMilliseconds;

            sw.Reset();
            sw.Start();
            for (int i = 0; i < iterations; i++)
            {
                var mongoOrder = await _mongoService.GetOrderByIdAsync(orderId);
            }
            sw.Stop();
            long mongoTime = sw.ElapsedMilliseconds;

            return Ok(new
            {
                Iterations = iterations,
                SqlTimeMs = sqlTime,
                MongoTimeMs = mongoTime,
                Winner = mongoTime < sqlTime ? "MongoDB (NoSQL)" : "MS SQL Server",
                FasterByPercent = sqlTime > 0 ? Math.Round((double)(sqlTime - mongoTime) / sqlTime * 100, 2) : 0
            });
        }
    }

}