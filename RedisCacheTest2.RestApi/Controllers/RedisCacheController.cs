using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using RedisCacheTest2.Lib.Extensions;

namespace RedisCacheTest2.RestApi.Controllers
{
    [Route("/redis-cache")]
    [ApiController]
    public class RedisCacheController : Controller
    {
        private readonly IDistributedCache _distributedCache;

        public RedisCacheController(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }
        [Route("/index")]
        [HttpGet]
        public IActionResult Index()
        {
            return Ok("Index Called");
        }
        [Route("/clear-all")]
        [HttpGet]
        public IActionResult ClearCache()
        {
            _distributedCache.ClearAll();
            return Ok("Clear Cache Called");
        }
    }
}
