using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using RedisCacheTest2.Lib.Cache;
using RedisCacheTest2.Lib.Extensions;

namespace RedisCacheTest2.RestApi.Controllers
{
    [Route("/redis-cache")]
    [ApiController]
    public class RedisCacheController : Controller
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IRedisCacheMem _redisCacheMem;

        public RedisCacheController(IDistributedCache distributedCache, IRedisCacheMem redisCacheMem)
        {
            _distributedCache = distributedCache;
            _redisCacheMem = redisCacheMem;
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
        [Route("/clear-all-second")]
        [HttpGet]
        public IActionResult ClearCacheSecond()
        {
            _redisCacheMem.ClearAll();
            return Ok("Clear Cache Second Called");
        }
    }
}
