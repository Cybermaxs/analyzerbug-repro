using System;
using System.Collections.Generic;
using System.Net;
using AnalyzerBug.Models;
using AnalyzerBug.Serialization;
using AnalyzerBug.Providers;
using AnalyzerBug.Providers.Remote;
using Moq;
using StackExchange.Redis;
using Xunit;
using AnalyzerBug.Tests.Attributes;
using AnalyzerBug.Client;

namespace AnalyzerBug.Tests.Providers
{
    public class RedisCacheProviderTests
    {
        private RedisCacheProvider service;
        private Mock<IConnectionMultiplexer> mux;
        private Mock<IServer> server;
        private Mock<IDatabase> db;

        public RedisCacheProviderTests()
        {
            var endpoints = new[]
            {
                new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1234)
            };

            var keys = new List<RedisKey>();
            keys.Add("key");

            var entry = new[]
            {
                new HashEntry("l_ttl", 2),
                new HashEntry("l_sld", 0),
                new HashEntry("l_rat", 0),
                new HashEntry("r_ttl", 5),
                new HashEntry("r_sld", 0),
                new HashEntry("r_rat", 0),
                new HashEntry("added", 635742110731968920),
                new HashEntry("data", "\"fcf87fd0-c497-4349-aa0d-31debdb6e539\"")
            };

            db = new Mock<IDatabase>();
            db.Setup(c => c.HashGetAllAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>())).ReturnsAsync(entry);
            db.Setup(c => c.HashGetAll(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>())).Returns(entry);
            db
                .Setup(c => c.KeyExistsAsync(
                    It.IsAny<RedisKey>(),
                    It.IsAny<CommandFlags>()
                    ))
                .ReturnsAsync(true);
            db
                .Setup(c => c.LockTake(
                    It.IsAny<RedisKey>(),
                    It.IsAny<RedisValue>(),
                    It.IsAny<TimeSpan>(),
                    It.IsAny<CommandFlags>()))
                .Returns(true);
            db
                .Setup(c => c.LockRelease(
                    It.IsAny<RedisKey>(),
                    It.IsAny<RedisValue>(),
                    It.IsAny<CommandFlags>()))
                .Returns(true);

            server = new Mock<IServer>();
            server.Setup(c => c.DatabaseSizeAsync(It.IsAny<int>(), It.IsAny<CommandFlags>())).ReturnsAsync(1L);
            server.Setup(c => c.IsSlave).Returns(false);
            server.Setup(c => c.IsConnected).Returns(true);
            server
                .Setup(c => c.Keys(
                    It.IsAny<int>(),
                    It.IsAny<RedisValue>(),
                    It.IsAny<int>(),
                    It.IsAny<long>(),
                    It.IsAny<int>(),
                    It.IsAny<CommandFlags>()
                    ))
                .Returns(keys);
            server
                .Setup(c => c.InfoRawAsync(
                    It.IsAny<RedisValue>(),
                    It.IsAny<CommandFlags>()
                    ))
                .ReturnsAsync("db1:keys=10,expires=10");

            mux = new Mock<IConnectionMultiplexer>();
            mux.Setup(c => c.IsConnected).Returns(true);
            mux.Setup(c => c.GetEndPoints(It.IsAny<bool>())).Returns(endpoints);
            mux.Setup(c => c.GetServer(It.IsAny<EndPoint>(), It.IsAny<object>())).Returns(server.Object);
            mux.Setup(c => c.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(db.Object);

            var settings = new ClientSettings();
            settings.DefaultRedisDb = 0;
            settings.Logger = new NullLogger();
            settings.AllowRemote = true;
            settings.RedisConnectionInfo = "localhost:6379";
            settings.MessageSerializer = new JsonMessageSerializer();

            service = new RedisCacheProvider(Guid.NewGuid(), new NullInvalidationService(), settings);
            service.mux = mux.Object;
            service.db = db.Object;
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTest)]
        public void GetNotifier()
        {
            var notifier = service.Notifier;
            Assert.NotNull(notifier);
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTest)]
        public void IsConnected()
        {
            Assert.True(service.IsConnected);
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTest)]
        public void Connect()
        {
            service.Connect();
            Assert.True(service.IsConnected);
        }

        [Theory]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTest)]
        [AutoMoqData]
        public void RemoveAsync(string key)
        {
            db.Setup(c => c.KeyDeleteAsync(It.IsAny<RedisKey[]>(), It.IsAny<CommandFlags>())).ReturnsAsync(1);

            Assert.Equal(1, service.RemoveAsync(key).Result);

            db.Verify(d => d.KeyDeleteAsync(new RedisKey[] { key }, It.IsAny<CommandFlags>()), Times.Once);
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTest)]
        public void FlushAsync()
        {
            service.FlushAsync();
        }

        [Trait(TestConstants.TestCategory, TestConstants.UnitTest)]
        [Fact]
        public void ContainsAsync()
        {
            Assert.True(service.ContainsAsync("key").Result);
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTest)]
        public void CountAsync()
        {
            Assert.Equal(service.CountAsync().Result, 10L);
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTest)]
        public void SetAsync()
        {
            var cacheItem = new RemoteCacheItem<string>("key", "value", ItemPolicy.Default, DateTime.Now);

            service.SetAsync(cacheItem);
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTest)]
        public void Set()
        {
            var cacheItem = new RemoteCacheItem<string>("key", "value", ItemPolicy.Default, DateTime.Now);

            service.Set(cacheItem);
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTest)]
        public void GetAsync()
        {
            var cacheItem = service.GetAsync<string>("key").Result;

            Assert.NotNull(cacheItem);
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTest)]
        public void Get()
        {
            var cacheItem = service.Get<string>("key");

            Assert.NotNull(cacheItem);
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTest)]
        public void LockTake()
        {
            Assert.True(service.LockTake("key", "value", TimeSpan.Zero));
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTest)]
        public void LockRelease()
        {
            Assert.True(service.LockRelease("key", "value"));
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTest)]
        public void Dispose()
        {
            service.Dispose();
        }
    }
}
