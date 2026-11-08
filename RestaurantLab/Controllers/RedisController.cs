using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Threading.Tasks;

namespace RestaurantLab.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RedisController : ControllerBase
    {
        private readonly IDatabase _redisDb;

        public RedisController(IConnectionMultiplexer redis)
        {
            _redisDb = redis.GetDatabase();
        }

        [HttpPost("set")]
        public async Task<IActionResult> SetKeyValue([FromQuery] string key, [FromQuery] string value)
        {
            await _redisDb.StringSetAsync(key, value);
            return Ok(new { message = "Значення успішно збережено в Redis", key, value });
        }

        [HttpGet("get/{key}")]
        public async Task<IActionResult> GetValue(string key)
        {
            RedisValue val = await _redisDb.StringGetAsync(key);
            if (val.IsNullOrEmpty)
            {
                return NotFound(new { error = $"Ключ '{key}' не знайдено в Redis" });
            }
            return Ok(new { key, value = val.ToString() });
        }

        [HttpDelete("delete/{key}")]
        public async Task<IActionResult> DeleteKey(string key)
        {
            bool removed = await _redisDb.KeyDeleteAsync(key);
            return Ok(new { key, removed });
        }
    }
}