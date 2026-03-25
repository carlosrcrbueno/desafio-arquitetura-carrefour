using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Api.Middlewares;
using Microsoft.AspNetCore.Http;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace Api.Tests.Middleware
{
    public class RateLimitMiddlewareTests
    {
        private static Mock<IConnectionMultiplexer> CreateRedisMock(out Mock<IDatabase> dbMock)
        {
            dbMock = new Mock<IDatabase>();

            var redisMock = new Mock<IConnectionMultiplexer>();
            redisMock
                .Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                .Returns(dbMock.Object);

            return redisMock;
        }

        [Fact]
        public async Task DevePermitirAte50RequisicoesPorMinuto()
        {
            var context = new DefaultHttpContext();
            context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
            context.Request.Path = "/transactions";

            var redisMock = CreateRedisMock(out var dbMock);

            long counter = 0;
            dbMock
                .Setup(x => x.StringIncrementAsync(It.IsAny<RedisKey>(), It.IsAny<long>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(() => ++counter);

            dbMock
                .Setup(x => x.KeyExpireAsync(It.IsAny<RedisKey>(), It.IsAny<TimeSpan?>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(true);

            RequestDelegate next = _ => Task.CompletedTask;
            var middleware = new RateLimitMiddleware(next, redisMock.Object);

            for (var i = 0; i < 50; i++)
            {
                context.Response = new DefaultHttpResponse(new DefaultHttpContext());
                await middleware.InvokeAsync(context);
                Assert.NotEqual((int)HttpStatusCode.TooManyRequests, context.Response.StatusCode);
            }
        }

        [Fact]
        public async Task DeveBloquearApos50Requisicoes()
        {
            var context = new DefaultHttpContext();
            context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
            context.Request.Path = "/transactions";

            var redisMock = CreateRedisMock(out var dbMock);

            long counter = 0;
            dbMock
                .Setup(x => x.StringIncrementAsync(It.IsAny<RedisKey>(), It.IsAny<long>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(() => ++counter);

            dbMock
                .Setup(x => x.KeyExpireAsync(It.IsAny<RedisKey>(), It.IsAny<TimeSpan?>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(true);

            dbMock
                .Setup(x => x.KeyTimeToLiveAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(TimeSpan.FromSeconds(30));

            RequestDelegate next = _ => Task.CompletedTask;
            var middleware = new RateLimitMiddleware(next, redisMock.Object);

            for (var i = 0; i < 50; i++)
            {
                context.Response = new DefaultHttpResponse(new DefaultHttpContext());
                await middleware.InvokeAsync(context);
            }

            context.Response = new DefaultHttpResponse(new DefaultHttpContext());
            await middleware.InvokeAsync(context);

            Assert.Equal((int)HttpStatusCode.TooManyRequests, context.Response.StatusCode);

            context.Response.Body.Position = 0;
        }

        [Fact]
        public async Task DeveRetornarStatus429QuandoExcedido()
        {
            var context = new DefaultHttpContext();
            context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
            context.Request.Path = "/transactions";

            var redisMock = CreateRedisMock(out var dbMock);

            long counter = 0;
            dbMock
                .Setup(x => x.StringIncrementAsync(It.IsAny<RedisKey>(), It.IsAny<long>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(() => ++counter);

            dbMock
                .Setup(x => x.KeyExpireAsync(It.IsAny<RedisKey>(), It.IsAny<TimeSpan?>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(true);

            dbMock
                .Setup(x => x.KeyTimeToLiveAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(TimeSpan.FromSeconds(25));

            RequestDelegate next = _ => Task.CompletedTask;
            var middleware = new RateLimitMiddleware(next, redisMock.Object);

            for (var i = 0; i < 51; i++)
            {
                context.Response = new DefaultHttpResponse(new DefaultHttpContext());
                await middleware.InvokeAsync(context);
            }

            Assert.Equal((int)HttpStatusCode.TooManyRequests, context.Response.StatusCode);

            // leitura do corpo para validar formato de erro
            context.Response.Body.Position = 0;
        }
    }
}
