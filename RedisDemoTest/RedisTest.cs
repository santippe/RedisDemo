using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RedisDemo;
using RedisDemo.Models;
using RedisWebApi.Models;
using StackExchange.Redis;

namespace RedisDemoTest
{
    public class RedisTest
    {
        private readonly RedisService _service;

        public RedisTest()
        {
            IConfiguration conf = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("System", LogLevel.Trace)
                    .AddConsole();
            });
            ILogger<RedisService> logger = loggerFactory.CreateLogger<RedisService>();
            var redisConf = conf.GetSection("RedisConf").Get<RedisConfiguration>();
            _service = new RedisService(logger, Options.Create(redisConf));
        }

        [Fact]

        public async Task TestDB()
        {
            await _service.TestDB();
        }

        [Fact]
        public void InsertObject()
        {
            var obj = new DumbObject() { Counter = 1, LstUpdt = DateTime.UtcNow };
            _service.InsertDbEntities("test", obj);
        }

        [Fact]
        public async Task MassiveInsertObjectAsync()
        {
            var parallelTask = Parallel.For(0, 100, async (i) =>
            {
                var obj = new DumbObject();
                obj.Counter = i;
                obj.LstUpdt = DateTime.UtcNow;
                _service.InsertDbEntities($"test", obj);
            });
            Assert.True(parallelTask.IsCompleted);
            var obj = await _service.GetDbEntities<DumbObject>("test");
            Assert.True(obj.Counter == 99);
        }

        [Fact]
        public async Task GetObjectAsync()
        {
            DumbObject obj = await _service.GetDbEntities<DumbObject>("test");
        }

        [Fact]
        public async Task DeleteObjectAsync()
        {
            var obj = new DumbObject()
            {
                Counter = 1,
                LstUpdt = DateTime.UtcNow
            };
            _service.InsertDbEntities<DumbObject>("test", obj);
            _service.DeleteDbEntities("test");
            var newObj = await _service.GetDbEntities<DumbObject>("test");

            Assert.Null(newObj);
        }

        [Fact]
        public async Task UpdateObjectAsync()
        {
            DumbObject obj = await _service.GetDbEntities<DumbObject>("test");

        }

        [Fact]
        public async Task GetAllKeysTestAsync()
        {
            var response = _service.GetAllDbEntities<DumbObject>();
        }
    }
}